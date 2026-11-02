using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi
{
    public class WShadow
    {
        public WLength OffsetX { get; private set; }
        public WLength OffsetY { get; private set; }
        public WLength Radius { get; private set; }
        public WLength SpreadRadius { get; private set; }
        public Color Color { get; private set; }
        public bool Inset { get; private set; }

        public WShadow(WLength offsetX, WLength offsetY, WLength radius, WLength spreadRadius, Color color = null, bool inset = false)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
            Radius = radius;
            SpreadRadius = spreadRadius;
            Color = color;
            Inset = inset;
        }

        //public WShadow(WLength offsetX, WLength offsetY, Color color = null, bool inset = false)
        //{
        //    OffsetX = offsetX;
        //    OffsetY = offsetY;
        //    Color = color;
        //    Inset = inset;
        //}

        public WShadow(WLength radius, Color color = null, bool inset = false)
        {
            Radius = radius;
            Color = color;
            Inset = inset;
        }

        public WShadow(WLength radius, WLength spreadRadius, Color color = null, bool inset = false)
        {
            Radius = radius;
            SpreadRadius = spreadRadius;
            Color = color;
            Inset = inset;
        }

        public void Write(StringBuilder builder)
        {
            if (Inset) {
                builder.Append(" inset ");
            }
            OffsetX.Write(builder);
            builder.Append(' ');
            OffsetY.Write(builder);
            if (Radius.Value != 0)
            {
                builder.Append(' ');
                Radius.Write(builder);
                if (SpreadRadius.Value != 0)
                {
                    builder.Append(' ');
                    SpreadRadius.Write(builder);
                }
            }

            if (Color != null)
            {
                builder.Append(Color.ToRgbaHex(true));
            }
        }
    }
}
