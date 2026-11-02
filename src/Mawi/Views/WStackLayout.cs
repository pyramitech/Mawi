using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Views
{
    public class WStackLayout : WContainer
    {
        private WStackOrientation _orientation = WStackOrientation.Vertical;
        private WLength _spacing = 0;

        public WStackOrientation Orientation { get => _orientation; set { _orientation = value; MarkUpdated(); } }
        public WLength Spacing { get => _spacing; set { _spacing = value; MarkUpdated(); } }

        public override void Render(StringBuilder sb)
        {
            this.UpdateState = UpdateState.None;

            sb.Append("<w-stack");
            RenderInternals(sb);
            RenderEvents(sb);
            sb.Append(" style=\\\"");
            RenderStyle(sb);


            sb.Append("\\\">");

            foreach (var child in Children)
            {                
                var view = child as WView;
                view.Render(sb);
            }
            sb.Append("</w-stack>");
        }

        /// <summary>
        /// this should always be called at the end of RenderStyle
        /// </summary>
        public override void RenderChildLayout(WView child, StringBuilder sb)
        {
            var isHorizontal = (_orientation == WStackOrientation.Horizontal || _orientation == WStackOrientation.HorizontalReverse);
            //sb.Append("flex: 0 0 auto;");

            var isStretchy = isHorizontal ? (child.HorizontalOptions.Alignment == LayoutAlignment.Fill &&
                child.WidthRequest == -1) :
                (child.VerticalOptions.Alignment == LayoutAlignment.Fill &&
                child.HeightRequest == -1);

            if (isStretchy)
            {
                sb.Append("flex: ");
                if (isHorizontal)
                {
                    sb.Append(child.MinimumWidth.Value != -1 ? "1 " : "0 ");
                    sb.Append(child.MaximumWidth.Value != double.PositiveInfinity ? "1 " : "0 ");
                }
                else
                {
                    sb.Append(child.MinimumHeight.Value != -1 ? "1 " : "0 ");
                    sb.Append(child.MaximumHeight.Value != double.PositiveInfinity ? "1 " : "0 ");
                }
                sb.Append("auto;");
            }
            else
            {
                sb.Append("flex: 0 0 auto;");
            }

            sb.Append("align-self:");
            switch (isHorizontal ? child.VerticalOptions.Alignment : child.HorizontalOptions.Alignment)
            {
                case LayoutAlignment.Fill:
                    if ((isHorizontal ? child.HeightRequest : child.WidthRequest) == -1)
                    {
                        sb.Append("stretch");
                    }
                    else
                    {
                        sb.Append("center");
                    }
                    break;

                case LayoutAlignment.Start:
                    sb.Append("start");
                    break;

                case LayoutAlignment.Center:
                    sb.Append("center");
                    break;

                case LayoutAlignment.End:
                    sb.Append("end");
                    break;
            }
            sb.Append(';');

            if (_orientation == WStackOrientation.Horizontal)
            {
                if (child.HorizontalOptions.Alignment == LayoutAlignment.End)
                {
                    sb.Append("margin-left: auto;");
                }                
            }
            else if (_orientation == WStackOrientation.Vertical)
            {
                if (child.VerticalOptions.Alignment == LayoutAlignment.End)
                {
                    sb.Append("margin-top: auto;");
                }
            }
            else if (_orientation == WStackOrientation.HorizontalReverse)
            {
                if (child.HorizontalOptions.Alignment == LayoutAlignment.End)
                {
                    sb.Append("margin-right: auto;");
                }
            }
            else if (_orientation == WStackOrientation.VerticalReverse)
            {
                if (child.VerticalOptions.Alignment == LayoutAlignment.End)
                {
                    sb.Append("margin-bottom: auto;");
                }
            }
        }

        public override void RenderStyle(StringBuilder sb)
        {
            sb.Append("display: flex;");

            sb.Append("flex-direction: ");

            switch (_orientation)
            {
                case WStackOrientation.Vertical:             
                    sb.Append("column;");
                    if (HeightRequest.Value == -1 && VerticalOptions.Alignment != LayoutAlignment.Fill)
                    {
                        sb.Append("height: max-content;");
                    }
                    sb.Append("row-gap:");
                    _spacing.Write(sb);
                    break;

                case WStackOrientation.Horizontal:
                    sb.Append("row;");
                    if (WidthRequest.Value == -1 && HorizontalOptions.Alignment != LayoutAlignment.Fill)
                    {
                        sb.Append("width: max-content;");
                    }
                    sb.Append("column-gap:");
                    _spacing.Write(sb);
                    break;

                case WStackOrientation.VerticalReverse:
                    sb.Append("column-reverse;");
                    if (HeightRequest.Value == -1 && VerticalOptions.Alignment != LayoutAlignment.Fill)
                    {
                        sb.Append("height: max-content;");
                    }
                    sb.Append("row-gap:");
                    _spacing.Write(sb);
                    break;

                case WStackOrientation.HorizontalReverse:
                    sb.Append("row-reverse;");
                    if (WidthRequest.Value == -1 && HorizontalOptions.Alignment != LayoutAlignment.Fill)
                    {
                        sb.Append("width: max-content;");
                    }
                    sb.Append("column-gap:");
                    _spacing.Write(sb);
                    break;
            }
            sb.Append(";");

            RenderBackground(sb);
            RenderPosition(sb);
            RenderTransform(sb);
            RenderBorder(sb);

            Parent.RenderChildLayout(this, sb);
        }
    }

    public enum WStackOrientation
    {
        Horizontal,
        Vertical,
        HorizontalReverse,
        VerticalReverse
    }
}
