using Microsoft.Maui.Controls.Xaml;
using System;
using System.Reflection;

namespace Mawi.Views
{
    public enum WBindingMode
    {
        /// <summary>TwoWay when the target property supports it (input controls), otherwise OneWay</summary>
        Default,
        OneWay,
        TwoWay,
        /// <summary>applies once when the BindingContext arrives, never subscribes to change notifications</summary>
        OneTime
    }

    /// <summary>
    /// lightweight binding for mawi views: <c>Text="{w:Bind Title}"</c>
    /// binds plain clr properties without any BindableProperty machinery,
    /// costs nothing unless used
    /// </summary>
    [ContentProperty(nameof(Path))]
    [RequireService(new[] { typeof(IProvideValueTarget) })]
    public class BindExtension : IMarkupExtension<object>
    {
        /// <summary>dotted property path resolved against the BindingContext (or <see cref="Source"/>)</summary>
        public string Path { get; set; }

        public WBindingMode Mode { get; set; } = WBindingMode.Default;

        public IValueConverter Converter { get; set; }

        public object ConverterParameter { get; set; }

        public string StringFormat { get; set; }

        /// <summary>explicit source object, overrides BindingContext</summary>
        public object Source { get; set; }

        /// <summary>value applied when the path cannot be resolved</summary>
        public object FallbackValue { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            var provideTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            if (provideTarget?.TargetObject is not WElement element)
            {
                throw new InvalidOperationException("w:Bind can only be used on properties of Mawi elements");
            }
            if (provideTarget.TargetProperty is not PropertyInfo property || !property.CanWrite)
            {
                throw new InvalidOperationException($"w:Bind on {element.GetType().Name} could not resolve a writable target property");
            }

            var expression = new WBindingExpression(element, property, this);
            element.AddBinding(expression);
            return expression.ProvideInitialValue();
        }
    }
}
