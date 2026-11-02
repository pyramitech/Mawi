using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi
{
    /// <summary>
    /// we need a custom struct here, because for whatever reason FlexBasis has IsAuto and IsRelative as internal...
    /// </summary>
    [TypeConverter(typeof(WFlexBasisTypeConverter))]
    public struct WFlexBasis : IEquatable<WFlexBasis>
    {
        bool _isLength;
        bool _isRelative;
        public float Length { get; }
        public bool IsAuto => !_isLength && !_isRelative;
        public bool IsRelative => _isRelative;

        public static readonly WFlexBasis Auto;

        public WFlexBasis(float length, bool isRelative = false)
        {
            if (length < 0)
                throw new ArgumentException("should be a positive value", nameof(length));
            if (isRelative && length > 1)
                throw new ArgumentException("relative length should be in [0, 1]", nameof(length));
            _isLength = !isRelative;
            _isRelative = isRelative;
            Length = length;
        }

        public static implicit operator WFlexBasis(float length)
            => new WFlexBasis(length);

        public bool Equals(WFlexBasis other) => _isLength == other._isLength && _isRelative == other._isRelative && Length == other.Length;

        public override bool Equals(object? obj) => obj is WFlexBasis other && Equals(other);

        public override int GetHashCode() => _isRelative.GetHashCode() ^ Length.GetHashCode();

        public static bool operator ==(WFlexBasis left, WFlexBasis right) => left.Equals(right);

        public static bool operator !=(WFlexBasis left, WFlexBasis right) => !(left == right);
    }

    public class WFlexBasisTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => sourceType == typeof(string) || sourceType == typeof(float);

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => destinationType == typeof(string);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is float floatValue)
            {
                return (FlexBasis)floatValue;
            }

            var strValue = value?.ToString();

            if (strValue != null)
            {
                strValue = strValue.Trim();

                if (strValue.Equals("auto", StringComparison.OrdinalIgnoreCase))
                    return FlexBasis.Auto;
                if (ParsePercentage(strValue, out float relflex))
                    return new FlexBasis(relflex / 100, isRelative: true);
                if (float.TryParse(strValue, NumberStyles.Number, CultureInfo.InvariantCulture, out float flex))
                    return new FlexBasis(flex);
            }

            throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", strValue, typeof(FlexBasis)));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is not WFlexBasis basis)
                throw new NotSupportedException();

            if (basis.IsAuto)
                return "auto";
            if (basis.IsRelative)
                return $"{(basis.Length * 100).ToString(CultureInfo.InvariantCulture)}%";

            return $"{basis.Length.ToString(CultureInfo.InvariantCulture)}";
        }

        static bool ParsePercentage(string strValue, out float relflex)
        {
            if (!strValue.EndsWith("%", StringComparison.OrdinalIgnoreCase))
            {
                relflex = default;
                return false;
            }

            var value =
#if NETSTANDARD2_0 // sigh
				strValue.Substring(0, strValue.Length - 1);
#else
                strValue.AsSpan(0, strValue.Length - 1);
#endif

            return float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out relflex);
        }
    }
}