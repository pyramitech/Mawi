using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi
{
    public enum CssBorderStyle
    {
        None,
        Solid,
        Dashed,
        Dotted,
        Double,
        Groove,
        Ridge,
        Inset,
        Outset
    }

    public static class CssBorderStyleExtensions
    {
        public static void Write(this CssBorderStyle style, StringBuilder sb)
        {
            switch (style)
            {
                case CssBorderStyle.None:
                    sb.Append("border-style: none;");
                    break;
                case CssBorderStyle.Solid:
                    sb.Append("border-style: solid;");
                    break;
                case CssBorderStyle.Dashed:
                    sb.Append("border-style: dashed;");
                    break;
                case CssBorderStyle.Dotted:
                    sb.Append("border-style: dotted;");
                    break;
                case CssBorderStyle.Double:
                    sb.Append("border-style: double;");
                    break;
                case CssBorderStyle.Groove:
                    sb.Append("border-style: groove;");
                    break;
                case CssBorderStyle.Ridge:
                    sb.Append("border-style: ridge;");
                    break;
                case CssBorderStyle.Inset:
                    sb.Append("border-style: inset;");
                    break;
                case CssBorderStyle.Outset:
                    sb.Append("border-style: outset;");
                    break;
                default:
                    sb.Append("border-style: solid;");
                    break;
            }
        }
    }
}
