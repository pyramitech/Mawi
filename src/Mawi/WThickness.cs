using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi
{
    [TypeConverter(typeof(WThicknessConverter))]
    public struct WThickness
    {
        public WLength Left;
        public WLength Top;
        public WLength Right;
        public WLength Bottom;

        public bool IsEmpty => Left.Value == 0 && Top.Value == 0 && Right.Value == 0 && Bottom.Value == 0;

        public WThickness(WLength left, WLength top, WLength right, WLength bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public WThickness(WLength uniform)
            : this(uniform, uniform, uniform, uniform)
        {
        }

        public WThickness(WLength horizontal, WLength vertical)
            : this(horizontal, vertical, horizontal, vertical)
        {
        }
        
        //implicit conversion from double
        public static implicit operator WThickness(double value) => new WThickness(value);

        public void Write(StringBuilder builder)
        {
            Top.Write(builder);
            builder.Append(' ');
            Right.Write(builder);
            builder.Append(' ');
            Bottom.Write(builder);
            builder.Append(' ');
            Left.Write(builder);
        }

        public override string ToString()
        {
            return $"{Top} {Right} {Bottom} {Left}";
        }

        public override bool Equals(object? obj)
        {
            if (obj is WThickness other)
            {
                return Left.Equals(other.Left) && Top.Equals(other.Top) &&
                       Right.Equals(other.Right) && Bottom.Equals(other.Bottom);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Left, Top, Right, Bottom);
        }
    }

        public class WThicknessConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo culture, object value)
        {
            if (value is string stringValue)
            {
                var parts = stringValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                switch (parts.Length)
                {
                    case 1: //uniform
                        var uniform = ParseLength(parts[0]);
                        return new WThickness(uniform);
                    case 2: //horizontal and vertical
                        var horizontal = ParseLength(parts[0]);
                        var vertical = ParseLength(parts[1]);
                        return new WThickness(horizontal, vertical);
                    case 4: //all sides
                        var left = ParseLength(parts[0]);
                        var top = ParseLength(parts[1]);
                        var right = ParseLength(parts[2]);
                        var bottom = ParseLength(parts[3]);
                        return new WThickness(left, top, right, bottom);
                }
            }

            return new WThickness(0);
           // throw new ArgumentException($"Cannot convert '{value}' to WThickness.");
        }

        private WLength ParseLength(string value)
        {
            var converter = new WLengthConverter();
            return (WLength)converter.ConvertFromInvariantString(value)!;
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is WThickness thickness)
            {
                return $"{thickness.Left} {thickness.Top} {thickness.Right} {thickness.Bottom}";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
