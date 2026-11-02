using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace Mawi
{
    [TypeConverter(typeof(WPointConverter))]
    public struct WPoint
    {
        public WLength X;
        public WLength Y;
        public WLength Z;

        public bool IsEmpty => X.Value == 0 && Y.Value == 0 && Z.Value == 0;

        public WPoint(WLength x, WLength y, WLength z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public WPoint(WLength x, WLength y)
        {
            X = x;
            Y = y;
        }

        public WPoint(WLength uniform)
            : this(uniform, uniform, uniform)
        {
        }

        //implicit conversion from double
        public static implicit operator WPoint(double value) => new WPoint(value);

        public void Write(StringBuilder builder)
        {
            X.Write(builder);
            builder.Append(' ');
            Y.Write(builder);
            if (Z.Value != 0)
            {
                builder.Append(' ');
                Z.Write(builder);
            }
        }

        public override string ToString()
        {
            return $"{X} {Y} {Z}";
        }

        public override bool Equals(object? obj)
        {
            if (obj is WPoint other)
            {
                return X.Equals(other.X) && Y.Equals(other.Y) &&
                       Z.Equals(other.Z);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }
    }

    public class WPointConverter : TypeConverter
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
                        return new WPoint(uniform);
                    case 3: //all sides
                        var x = ParseLength(parts[0]);
                        var y = ParseLength(parts[1]);
                        var z = ParseLength(parts[2]);
                        return new WPoint(x, y, z);
                }
            }

            return new WPoint();
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
            if (destinationType == typeof(string) && value is WPoint point)
            {
                return $"{point.X} {point.Y} {point.Z}";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
