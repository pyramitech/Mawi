using Mawi.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Animation
{
    public class WKeyframe
    {
        internal class WProperty
        {
            internal string Name;
            internal string Value;
            internal PropertyInfo Property;
            internal object OriginalValue;
        }

        private List<WProperty> _properties = new();
        private List<WProperty> _transformProperties = new();
        public double Offset { get; set; }

        internal void ProcessProperties(Dictionary<string, WKeyframe.WProperty> props)
        {
            foreach (var prop in _properties)
            {
                props[prop.Name] = prop;
            }
            foreach (var prop in _transformProperties)
            {
                props[prop.Name] = prop;
            }
        }

        public void Write(StringBuilder sb)
        {
            sb.Append('{');
            bool first = true;
            if (Offset != 0)
            {
                sb.Append("offset:").Append(Offset.ToString(CultureInfo.InvariantCulture));
                first = false;
            }
            if (_transformProperties.Count > 0)
            {
                if (!first)
                {
                    sb.Append(',');
                }
                first = false;
                sb.Append("transform:'");
                for (int i = 0; i < _transformProperties.Count; i++)
                {
                    sb.Append(' ');
                    var prop = _transformProperties[i];
                    sb.Append(prop.Name).Append('(').Append(prop.Value).Append(')');
                }
                sb.Append('\'');
            }
            for (int i = 0; i < _properties.Count; i++)
            {
                var prop = _properties[i];
                if (!first)
                {
                    sb.Append(',');
                }
                first = false;
                sb.Append(prop.Name).Append(":'").Append(prop.Value).Append('\'');
            }
            sb.Append('}');
        }
   

        public WKeyframe AddProperty<TView, TProperty>(Expression<Func<TView, TProperty>> propertySelector, TProperty value) where TView : WView
        {
            if (propertySelector.Body is MemberExpression memberExpression)
            {
                var member = memberExpression.Member;
                
                var attribute = member.GetCustomAttribute<CssNameAttribute>();
                if (attribute != null)
                {
                    var prop = new WProperty();
                    prop.Name = attribute.Name;
                    prop.Value = value.ToString() + attribute.Unit;
                    if (member is PropertyInfo property)
                    {
                        prop.Property = property;
                        prop.OriginalValue = value;
                    }
                    if (attribute.Transform)
                    {
                        _transformProperties.Add(prop);
                    }
                    else
                    {
                        _properties.Add(prop);
                    }
                }
                return this;
            }
            throw new InvalidOperationException("Provided property is not animatable");
        }

        public WKeyframe AddProperty<TView>(Expression<Func<TView, WThickness>> propertySelector, WThickness value) where TView : WView
        {
            if (propertySelector.Body is MemberExpression memberExpression)
            {
                var member = memberExpression.Member;
                var attribute = member.GetCustomAttribute<CssNameAttribute>();
                if (attribute != null)
                {
                    var prop = new WProperty();
                    prop.Name = attribute.Name;
                    prop.Value = value.ToString();
                    if (member is PropertyInfo property)
                    {
                        prop.Property = property;
                        prop.OriginalValue = value;
                    }
                    if (attribute.Transform)
                    {
                        _transformProperties.Add(prop);
                    }
                    else
                    {
                        _properties.Add(prop);
                    }
                }
                return this;
            }
            throw new InvalidOperationException("Provided property is not animatable");
        }



        public WKeyframe AddProperty<TView>(Expression<Func<TView, WLength>> propertySelector, WLength value) where TView : WView
        {
            if (propertySelector.Body is MemberExpression memberExpression)
            {
                var member = memberExpression.Member;
                var attribute = member.GetCustomAttribute<CssNameAttribute>();
                if (attribute != null)
                {
                    var prop = new WProperty();
                    prop.Name = attribute.Name;
                    prop.Value = value.ToString();
                    if (member is PropertyInfo property)
                    {
                        prop.Property = property;
                        prop.OriginalValue = value;
                    }
                    if (attribute.Transform)
                    {
                        _transformProperties.Add(prop);
                    }
                    else
                    {
                        _properties.Add(prop);
                    }
                }
                return this;
            }
            throw new InvalidOperationException("Provided property is not animatable");
        }

        public WKeyframe AddProperty<TView>(Expression<Func<TView, double>> propertySelector, double value) where TView : WView
        {
            if (propertySelector.Body is MemberExpression memberExpression)
            {
                var member = memberExpression.Member;
                var attribute = member.GetCustomAttribute<CssNameAttribute>();
                if (attribute != null)
                {
                    var prop = new WProperty();
                    prop.Name = attribute.Name;
                    prop.Value = value.ToString(CultureInfo.InvariantCulture) + attribute.Unit;
                    if (member is PropertyInfo property)
                    {
                        prop.Property = property;
                        prop.OriginalValue = value;
                    }
                    if (attribute.Transform)
                    {
                        _transformProperties.Add(prop);
                    }
                    else
                    {
                        _properties.Add(prop);
                    }
                }
                return this;
            }
            throw new InvalidOperationException("Provided property is not animatable");
        }

        public WKeyframe AddProperty<TView>(Expression<Func<TView, int>> propertySelector, int value) where TView : WView
        {
            if (propertySelector.Body is MemberExpression memberExpression)
            {
                var member = memberExpression.Member;
                var attribute = member.GetCustomAttribute<CssNameAttribute>();
                if (attribute != null)
                {
                    var prop = new WProperty();
                    prop.Name = attribute.Name;
                    prop.Value = value.ToString(CultureInfo.InvariantCulture) + attribute.Unit;
                    if (member is PropertyInfo property)
                    {
                        prop.Property = property;
                        prop.OriginalValue = value;
                    }
                    if (attribute.Transform)
                    {
                        _transformProperties.Add(prop);
                    }
                    else
                    {
                        _properties.Add(prop);
                    }
                }
                return this;
            }
            throw new InvalidOperationException("Provided property is not animatable");
        }

        public WKeyframe AddProperty<TView>(Expression<Func<TView, Color>> propertySelector, Color value) where TView: WView
        {
            if (propertySelector.Body is MemberExpression memberExpression)
            {
                var member = memberExpression.Member;
                var attribute = member.GetCustomAttribute<CssNameAttribute>();
                if (attribute != null)
                {
                    var prop = new WProperty();
                    prop.Name = attribute.Name;
                    prop.Value = value.ToRgbaHex(true);
                    if (member is PropertyInfo property)
                    {
                        prop.Property = property;
                        prop.OriginalValue = value;
                    }
                    if (attribute.Transform)
                    {
                        _transformProperties.Add(prop);
                    }
                    else
                    {
                        _properties.Add(prop);
                    }
                }
                return this;
            }
            throw new InvalidOperationException("Provided property is not animatable");
        }
    }
}
