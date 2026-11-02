using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Views
{
    public class WScrollView : WContent
    {
        public WScrollView()
        {
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;
        }

        private ScrollBarVisibility _scrollBarVisibility;
        private ScrollOrientation _orientation;

        public double ScrollX => _scrollX;
        public double ScrollY => _scrollY;
        public double ScrollWidth => _scrollWidth;
        public double ScrollHeight => _scrollHeight;

        public ScrollBarVisibility ScrollBarVisibility { get => _scrollBarVisibility; set { _scrollBarVisibility = value; MarkUpdated(); } }
        public ScrollOrientation Orientation { get => _orientation; set { _orientation = value; MarkUpdated(); } }

        public void ScrollTo(double x, double y, ScrollBehavior behavior = ScrollBehavior.Auto)
        {
            string type = behavior switch
            {
                ScrollBehavior.Instant => "instant",
                ScrollBehavior.Smooth => "smooth",
                _ => "auto",
            };
            Root.Execute($"scrollTo(\"w{Id}\", {x}, {y}, \"{type}\");");
        }

        public override void Render(StringBuilder sb)
        {
            this.UpdateState = UpdateState.None;

            sb.Append("<w-scroll");
            RenderInternals(sb);
            RenderEvents(sb);
            sb.Append(" style=\\\"");
            RenderStyle(sb);
            sb.Append("\\\">");

            if (Content != null)
            {
                Content.Render(sb);
            }

            sb.Append("</w-scroll>");
        }

        public override void RenderChildLayout(WView child, StringBuilder sb)
        {
            //technically nothing to do here I guess? We shouldn't affect the layout at all since all we do is just wrap the content..
        }

        private void WriteScrollBarStyle(ScrollBarVisibility visibility, StringBuilder sb)
        {
            switch (visibility)
            {
                case ScrollBarVisibility.Default:
                    sb.Append("auto");
                    break;

                case ScrollBarVisibility.Always:
                    sb.Append("scroll");
                    break;

                case ScrollBarVisibility.Never:
                    sb.Append("hidden");
                    break;
            }
        }

        public override void RenderStyle(StringBuilder sb)
        {
            sb.Append("display:block;");

            RenderBackground(sb);
            RenderPosition(sb);
            RenderTransform(sb);
            RenderBorder(sb);

            if (_orientation == ScrollOrientation.Both)
            {
                sb.Append("overflow:");
                if (_scrollBarVisibility == ScrollBarVisibility.Default)
                {
                    sb.Append("auto");
                }
                else
                {
                    sb.Append("scroll");
                }
                sb.Append(';');
            }
            else if (_orientation == ScrollOrientation.Vertical)
            {
                sb.Append("overflow-y:");
                if (_scrollBarVisibility == ScrollBarVisibility.Default)
                {
                    sb.Append("auto");
                }
                else
                {
                    sb.Append("scroll");
                }
                sb.Append(';');
            }
            else if (_orientation == ScrollOrientation.Horizontal)
            {
                sb.Append("overflow-x:");
                if (_scrollBarVisibility == ScrollBarVisibility.Default)
                {
                    sb.Append("auto");
                }
                else
                {
                    sb.Append("scroll");
                }
                sb.Append(';');
            }

            if (_scrollBarVisibility == ScrollBarVisibility.Never)
            {
                sb.Append("scrollbar-width: none;");
            }

            Parent.RenderChildLayout(this, sb);
        }
    }

    public enum ScrollBehavior
    {
        Auto,
        Instant,
        Smooth
    }

    public enum ScrollAlignment
    {
        Start,
        Center,
        End,
        Nearest
    }
}
