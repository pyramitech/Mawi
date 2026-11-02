using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Mawi.Views
{
    /// <summary>
    /// a single active {w:Bind} on a view. resolves a dotted property path against the
    /// BindingContext with cached reflection, subscribes INotifyPropertyChanged per path hop
    /// (weakly, so a long-lived viewmodel cannot keep a discarded view alive) and writes
    /// the resolved value into the plain clr target property
    /// </summary>
    public sealed class WBindingExpression
    {
        private static readonly ConcurrentDictionary<(Type, string), PropertyInfo> _accessorCache = new();
        private static readonly object DoesNotExist = new();

        private readonly WElement _target;
        private readonly PropertyInfo _targetProperty;
        private readonly string[] _path;
        private readonly WBindingMode _mode;
        private readonly IValueConverter _converter;
        private readonly object _converterParameter;
        private readonly string _stringFormat;
        private readonly object _source;
        private readonly object _fallbackValue;

        private WeakPropertyChangedProxy[] _proxies;
        private bool _applying;
        private bool _targetListenerAttached;

        internal WBindingExpression(WElement target, PropertyInfo targetProperty, BindExtension bind)
        {
            _target = target;
            _targetProperty = targetProperty;
            _path = string.IsNullOrEmpty(bind.Path) || bind.Path == "."
                ? Array.Empty<string>()
                : bind.Path.Split('.');
            _converter = bind.Converter;
            _converterParameter = bind.ConverterParameter;
            _stringFormat = bind.StringFormat;
            _source = bind.Source;
            _fallbackValue = bind.FallbackValue;

            _mode = bind.Mode == WBindingMode.Default
                ? (SupportsTargetListener() ? WBindingMode.TwoWay : WBindingMode.OneWay)
                : bind.Mode;
        }

        /// <summary>
        /// value returned from ProvideValue at inflation time. if a context is already
        /// available the binding is fully applied first, otherwise the property keeps
        /// its current value until the BindingContext arrives
        /// </summary>
        internal object ProvideInitialValue()
        {
            if ((_source ?? _target.BindingContext) != null)
            {
                Apply();
            }
            return _targetProperty.GetValue(_target);
        }

        internal void Apply()
        {
            if (!MainThread.IsMainThread)
            {
                //viewmodels are allowed to raise PropertyChanged from any thread
                MainThread.BeginInvokeOnMainThread(Apply);
                return;
            }

            DetachSourceListeners();

            var source = _source ?? _target.BindingContext;

            var value = source == null
                ? DoesNotExist
                : Resolve(source, subscribe: _mode != WBindingMode.OneTime);

            if (value == DoesNotExist)
            {
                if (_fallbackValue == null)
                    return;
                value = _fallbackValue;
            }

            SetTargetValue(value);

            if (_mode == WBindingMode.TwoWay)
            {
                AttachTargetListener();
            }
        }

        private object Resolve(object source, bool subscribe)
        {
            object current = source;
            for (int i = 0; i < _path.Length; i++)
            {
                if (current == null)
                    return DoesNotExist;

                if (subscribe && current is INotifyPropertyChanged inpc)
                {
                    _proxies ??= new WeakPropertyChangedProxy[_path.Length];
                    _proxies[i] = new WeakPropertyChangedProxy(this, inpc, _path[i]);
                }

                var prop = GetAccessor(current.GetType(), _path[i]);
                if (prop == null)
                    return DoesNotExist;

                current = prop.GetValue(current);
            }
            return current;
        }

        private static PropertyInfo GetAccessor(Type type, string name)
            => _accessorCache.GetOrAdd((type, name), static key =>
            {
                try
                {
                    return key.Item1.GetProperty(key.Item2, BindingFlags.Public | BindingFlags.Instance);
                }
                catch (AmbiguousMatchException)
                {
                    //shadowed property, most derived wins
                    return key.Item1.GetProperty(key.Item2, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        ?? key.Item1.BaseType?.GetProperty(key.Item2, BindingFlags.Public | BindingFlags.Instance);
                }
            });

        private void DetachSourceListeners()
        {
            if (_proxies == null)
                return;

            for (int i = 0; i < _proxies.Length; i++)
            {
                _proxies[i]?.Detach();
                _proxies[i] = null;
            }
        }

        internal void OnSourcePropertyChanged(string propertyName, string segmentName)
        {
            if (_applying)
                return;

            if (string.IsNullOrEmpty(propertyName) || propertyName == segmentName)
            {
                Apply();
            }
        }

        private void SetTargetValue(object value)
        {
            try
            {
                if (_converter != null)
                {
                    value = _converter.Convert(value, _targetProperty.PropertyType, _converterParameter, CultureInfo.CurrentCulture);
                }

                if (_stringFormat != null)
                {
                    value = string.Format(CultureInfo.CurrentCulture, _stringFormat, value);
                }

                value = CoerceToTargetType(value, _targetProperty.PropertyType);

                _applying = true;
                if (!Equals(_targetProperty.GetValue(_target), value))
                {
                    _targetProperty.SetValue(_target, value);
                }
            }
            catch (Exception ex)
            {
                WRoot.ErrorDebugAction?.Invoke(ex);
            }
            finally
            {
                _applying = false;
            }
        }

        private static object CoerceToTargetType(object value, Type targetType)
        {
            if (value == null)
            {
                return targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null
                    ? Activator.CreateInstance(targetType)
                    : null;
            }

            if (targetType.IsInstanceOfType(value))
                return value;

            if (targetType == typeof(string))
                return value.ToString();

            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (underlying.IsEnum)
            {
                return value is string s
                    ? Enum.Parse(underlying, s, true)
                    : Enum.ToObject(underlying, value);
            }

            return Convert.ChangeType(value, underlying, CultureInfo.CurrentCulture);
        }

        #region target -> source

        private bool SupportsTargetListener()
            => _target is WEntryBase && _targetProperty.Name == nameof(WEntryBase.Text);

        private void AttachTargetListener()
        {
            if (_targetListenerAttached || !SupportsTargetListener())
                return;

            //subscribing TextChanged also activates the js input event on the element
            var entry = (WEntryBase)_target;
            entry.TextChanged += (s, e) => PushToSource(entry.Text);
            _targetListenerAttached = true;
        }

        private void PushToSource(object value)
        {
            if (_applying || _path.Length == 0)
                return;

            try
            {
                object leaf = _source ?? _target.BindingContext;
                if (leaf == null)
                    return;

                for (int i = 0; i < _path.Length - 1; i++)
                {
                    var hop = GetAccessor(leaf.GetType(), _path[i]);
                    leaf = hop?.GetValue(leaf);
                    if (leaf == null)
                        return;
                }

                var prop = GetAccessor(leaf.GetType(), _path[^1]);
                if (prop == null || !prop.CanWrite)
                    return;

                if (_converter != null)
                {
                    value = _converter.ConvertBack(value, prop.PropertyType, _converterParameter, CultureInfo.CurrentCulture);
                }
                value = CoerceToTargetType(value, prop.PropertyType);

                _applying = true;
                try
                {
                    if (!Equals(prop.GetValue(leaf), value))
                    {
                        prop.SetValue(leaf, value);
                    }
                }
                finally
                {
                    _applying = false;
                }
            }
            catch (Exception ex)
            {
                WRoot.ErrorDebugAction?.Invoke(ex);
            }
        }

        #endregion
    }

    /// <summary>
    /// subscribes to a source's PropertyChanged while holding the binding expression weakly,
    /// so a long-lived viewmodel never keeps a discarded view alive. detaches itself the
    /// first time it fires after the expression has been collected
    /// </summary>
    internal sealed class WeakPropertyChangedProxy
    {
        private readonly WeakReference<WBindingExpression> _expression;
        private readonly INotifyPropertyChanged _source;
        private readonly string _segmentName;

        public WeakPropertyChangedProxy(WBindingExpression expression, INotifyPropertyChanged source, string segmentName)
        {
            _expression = new WeakReference<WBindingExpression>(expression);
            _source = source;
            _segmentName = segmentName;
            source.PropertyChanged += OnPropertyChanged;
        }

        public void Detach() => _source.PropertyChanged -= OnPropertyChanged;

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_expression.TryGetTarget(out var expression))
            {
                expression.OnSourcePropertyChanged(e.PropertyName, _segmentName);
            }
            else
            {
                Detach();
            }
        }
    }
}
