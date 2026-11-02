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
    [TypeConverter(typeof(WLengthConverter))]
    public struct WLength
    {
        public double Value;
        public LengthUnit Unit;

        public WLength(double value, LengthUnit unit = LengthUnit.Px)
        {
            Value = value;
            Unit = unit;
        }

        //implicit conversion from double
        public static implicit operator WLength(double value) => new WLength(value);

        public static bool operator ==(WLength length, double value) => length.Value == value;
        public static bool operator !=(WLength length, double value) => length.Value != value;

        public static bool operator <(WLength length, double value) => length.Value < value;
        public static bool operator >(WLength length, double value) => length.Value > value;
        public static bool operator <=(WLength length, double value) => length.Value <= value;
        public static bool operator >=(WLength length, double value) => length.Value >= value;

        public static bool operator ==(WLength left, WLength right) => left.Value == right.Value && left.Unit == right.Unit;
        public static bool operator !=(WLength left, WLength right) => !(left == right);

        public override string ToString()
        {
            return Unit switch
            {
                LengthUnit.Px => $"{Value.ToInv()}px",
                LengthUnit.Em => $"{Value.ToInv()}em",
                LengthUnit.Rem => $"{Value.ToInv()}rem",
                LengthUnit.Percent => $"{Value.ToInv()}%",
                LengthUnit.Vw => $"{Value.ToInv()}vw",
                LengthUnit.Vh => $"{Value.ToInv()}vh",
                LengthUnit.Cm => $"{Value.ToInv()}cm",
                LengthUnit.Mm => $"{Value.ToInv()}mm",
                LengthUnit.In => $"{Value.ToInv()}in",
                LengthUnit.Pt => $"{Value.ToInv()}pt",
                LengthUnit.Pc => $"{Value.ToInv()}pc",
                _ => throw new NotSupportedException($"Unknown LengthUnit: {Unit}")
            };
        }

        public void Write(StringBuilder builder)
        {
            builder.Append(Value.ToInv());
            WriteUnit(builder);
        }

        public void WriteUnit(StringBuilder builder)
        {
            switch (Unit)
            {
                case LengthUnit.Px:
                    builder.Append("px");
                    break;
                case LengthUnit.Em:
                    builder.Append("em");
                    break;
                case LengthUnit.Rem:
                    builder.Append("rem");
                    break;
                case LengthUnit.Percent:
                    builder.Append('%'); //append % for Percent
                    break;
                case LengthUnit.Vw:
                    builder.Append("vw");
                    break;
                case LengthUnit.Vh:
                    builder.Append("vh");
                    break;
                case LengthUnit.Cm:
                    builder.Append("cm");
                    break;
                case LengthUnit.Mm:
                    builder.Append("mm");
                    break;
                case LengthUnit.In:
                    builder.Append("in");
                    break;
                case LengthUnit.Pt:
                    builder.Append("pt");
                    break;
                case LengthUnit.Pc:
                    builder.Append("pc");
                    break;
                default:
                    throw new NotSupportedException($"Unknown LengthUnit: {Unit}");
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is WLength other)
            {
                return Value.Equals(other.Value) && Unit == other.Unit;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Unit);
        }
    }

    public class WLengthConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public static WLength ParseFromString(string stringValue)
        {
            stringValue = stringValue.Trim(); //trim any whitespace

            //special for perc
            if (stringValue.EndsWith("%", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(stringValue[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                {
                    return new WLength(number, LengthUnit.Percent);
                }
            }

            //all other units
            foreach (LengthUnit unit in Enum.GetValues(typeof(LengthUnit)))
            {
                string unitStr = unit.ToString().ToLower();
                if (stringValue.EndsWith(unitStr, StringComparison.OrdinalIgnoreCase))
                {
                    if (double.TryParse(stringValue[..^unitStr.Length], NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                    {
                        return new WLength(number, unit);
                    }
                    break;
                }
            }

            //default pixels
            if (double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var plainNumber))
            {
                return new WLength(plainNumber, LengthUnit.Px);
            }

            return new WLength(0);
        }

        public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string stringValue)
            {
                return ParseFromString(stringValue);
            }

            return new WLength(0); //TOOD: log?
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is WLength length)
            {
                return length.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public enum LengthUnit : byte
    {
        Px,      //pixels
        Em,      //relative to font size
        Rem,     //relative to root font size
        Percent, //percentage
        Vw,      //viewport width
        Vh,      //viewport height
        Cm,      //centimeters
        Mm,      //millimeters
        In,      //inches
        Pt,      //points
        Pc,      //picas
    }
}
