using Mawi.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi
{
    [TypeConverter(typeof(Point3Converter))]
    public struct Point3
    {
        public double X;
        public double Y;
        public double Z;

        public Point3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Point3(double uniform)
            : this(uniform, uniform, uniform)
        {
        }

        //implicit conversion from double
        public static implicit operator Point3(double value) => new Point3(value);

        public void Write(StringBuilder builder)
        {
            builder.Append(X.ToInv());
            builder.Append(' ');
            builder.Append(Y.ToInv());
            builder.Append(' ');
            builder.Append(Z.ToInv());
        }

        public override string ToString()
        {
            return $"{X.ToInv()} {Y.ToInv()} {Z.ToInv()}";
        }

        public override bool Equals(object? obj)
        {
            if (obj is Point3 other)
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

    public class Point3Converter : TypeConverter
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
                        var uniform = double.Parse(parts[0]);
                        return new Point3(uniform);
                    case 3: //all sides
                        var x = double.Parse(parts[0]);
                        var y = double.Parse(parts[1]);
                        var z = double.Parse(parts[2]);
                        return new Point3(x, y, z);
                }
            }

            return new Point3();
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Point3 point)
            {
                return $"{point.X} {point.Y} {point.Z}";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
