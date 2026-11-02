using Mawi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi
{
    public abstract class WFilter
    {
        public abstract void Write(StringBuilder builder);
    }

    public class WBlurFilter : WFilter
    {
        public WLength Radius { get; }

        public WBlurFilter(WLength radius)
        {
            Radius = radius;
        }

        public override void Write(StringBuilder builder)
        {
            builder.Append("blur(");
            Radius.Write(builder);
            builder.Append(")");
        }
    }

    public class WBrightnessFilter : WFilter
    {
        public double Amount { get; }

        public WBrightnessFilter(double amount)
        {
            Amount = amount;
        }

        public override void Write(StringBuilder builder)
        {
            builder.Append("brightness(").Append((Amount * 100).ToInv()).Append("%)");
        }
    }

    public class WContrastFilter : WFilter
    {
        public double Amount { get; }

        public WContrastFilter(double amount)
        {
            Amount = amount;
        }

        public override void Write(StringBuilder builder)
        {
            builder.Append("contrast(").Append((Amount * 100).ToInv()).Append("%)");
        }
    }

    public class WDropShadowFilter : WFilter
    {
        public WLength OffsetX { get; private set; }
        public WLength OffsetY { get; private set; }
        public WLength Radius { get; private set; }
        public Color Color { get; private set; }

        public WDropShadowFilter(WLength radius, Color color = null)
        {
            Radius = radius;
            Color = color;
        }

        public WDropShadowFilter(WLength offsetX, WLength offsetY, Color color = null)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
            Color = color;
        }

        public WDropShadowFilter(WLength offsetX, WLength offsetY, WLength radius, Color color = null)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
            Radius = radius;
            Color = color;
        }

        public override void Write(StringBuilder builder)
        {
            builder.Append("drop-shadow(");
            OffsetX.Write(builder);
            builder.Append(' ');
            OffsetY.Write(builder);

            if (Radius.Value != 0)
            {
                builder.Append(' ');
                Radius.Write(builder);
            }

            if (Color != null)
            {
                builder.Append(' ');
                builder.Append(Color.ToRgbaHex(true));
            }

            builder.Append(")");
        }
    }

    public class WGrayscaleFilter : WFilter
    {
        public double Amount { get; }

        public WGrayscaleFilter(double amount)
        {
            Amount = amount;
        }

        public override void Write(StringBuilder builder)
        {
            builder.Append("grayscale(").Append((Amount * 100).ToInv()).Append("%)");
        }
    }

    public class WHueRotateFilter : WFilter
    {
        public double Angle { get; }

        public WHueRotateFilter(double angle)
        {
            Angle = angle;
        }

        public override void Write(StringBuilder builder)
        {
            builder.Append("hue-rotate(").Append(Angle.ToInv()).Append("deg)");
        }
    }

    public class WInvertFilter : WFilter
    {
        public double Amount { get; }

        public WInvertFilter(double amount)
        {
            Amount = amount;
        }

        public override void Write(StringBuilder builder)
        {
            builder.Append("invert(").Append((Amount * 100).ToInv()).Append("%)");
        }
    }
    public class WOpacityFilter : WFilter
    {
        public double Amount { get; }

        public WOpacityFilter(double amount)
        {
            Amount = amount;
        }

        public override void Write(StringBuilder builder)
        {
            builder.Append("opacity(").Append((Amount * 100).ToInv()).Append("%)");
        }
    }
    public class WSepiaFilter : WFilter
    {
        public double Amount { get; }

        public WSepiaFilter(double amount)
        {
            Amount = amount;
        }

        public override void Write(StringBuilder builder)
        {
            builder.Append("sepia(").Append((Amount * 100).ToInv()).Append("%)");
        }
    }

    public class WSaturateFilter : WFilter
    {
        public double Amount { get; }

        public WSaturateFilter(double amount)
        {
            Amount = amount;
        }

        public override void Write(StringBuilder builder)
        {
            builder.Append("saturate(").Append((Amount * 100).ToInv()).Append("%)");
        }
    }

}
