using Mawi.Helpers;
using System.Text;

namespace Mawi
{
    /// <summary>
    /// typed scrollbar theme, compiled to css. scrollbar pseudo-elements cannot be styled
    /// inline, so themes are applied as stylesheets: globally via WRoot.SetScrollbarTheme,
    /// or scoped by rendering ToCss with a selector (".my-class") and loading the result
    /// with WRoot.LoadStyle, then putting the class on the target views via StyleClasses.
    ///
    /// deliberately emits only ::-webkit-scrollbar rules and never the standard
    /// scrollbar-width/scrollbar-color properties: chromium ignores the webkit pseudo
    /// styling entirely once a standard property is present, so mixing them would silently
    /// disable the theme on android and windows. ios shows its native overlay scrollbars
    /// and ignores all of this by design
    /// </summary>
    public class WScrollbarTheme
    {
        /// <summary>scrollbar thickness in pixels, both axes</summary>
        public double Thickness = 8;

        public Color ThumbColor;
        public Color ThumbHoverColor;
        public Color TrackColor;

        /// <summary>corner radius of the thumb in pixels</summary>
        public double ThumbRadius;

        /// <summary>inset between thumb and track edge in pixels, rendered as a track-colored border around the thumb</summary>
        public double ThumbMargin;

        public string ToCss(string scope = "*")
        {
            var sb = new StringBuilder(256);

            sb.Append(scope).Append("::-webkit-scrollbar{width:").Append(Thickness.ToInv())
                .Append("px;height:").Append(Thickness.ToInv()).Append("px;");
            if (TrackColor != null)
            {
                sb.Append("background-color:").Append(TrackColor.ToRgbaHex(true)).Append(';');
            }
            sb.Append('}');

            sb.Append(scope).Append("::-webkit-scrollbar-thumb{");
            if (ThumbColor != null)
            {
                sb.Append("background-color:").Append(ThumbColor.ToRgbaHex(true)).Append(';');
            }
            if (ThumbRadius > 0)
            {
                sb.Append("border-radius:").Append(ThumbRadius.ToInv()).Append("px;");
            }
            if (ThumbMargin > 0)
            {
                //background-clip keeps the thumb color out of the border area, so the border
                //acts as an inset regardless of track color (including transparent)
                sb.Append("border:").Append(ThumbMargin.ToInv()).Append("px solid transparent;");
                sb.Append("background-clip:content-box;");
            }
            sb.Append('}');

            if (ThumbHoverColor != null)
            {
                sb.Append(scope).Append("::-webkit-scrollbar-thumb:hover{background-color:")
                    .Append(ThumbHoverColor.ToRgbaHex(true)).Append(";}");
            }

            if (TrackColor != null)
            {
                sb.Append(scope).Append("::-webkit-scrollbar-track{background-color:")
                    .Append(TrackColor.ToRgbaHex(true)).Append(";}");
            }

            sb.Append(scope).Append("::-webkit-scrollbar-corner{background-color:")
                .Append(TrackColor != null ? TrackColor.ToRgbaHex(true) : "transparent").Append(";}");

            return sb.ToString();
        }
    }
}
