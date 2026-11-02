using System.Text;

namespace Mawi.Views
{
    /// <summary>
    /// lightweight single-child wrapper: gives the child the full alignment model
    /// (start/center/end/fill on both axes) without the cost of a WGrid — no track
    /// templates, no grid placement styles and no layout settings allocation.
    /// renders as a single implicit grid cell
    /// </summary>
    public class WFrame : WContent
    {
        public override void Render(StringBuilder sb)
        {
            this.UpdateState = UpdateState.None;

            sb.Append("<w-frame");
            RenderInternals(sb);
            RenderEvents(sb);
            sb.Append(" style=\\\"");
            RenderStyle(sb);
            sb.Append("\\\">");

            _content?.Render(sb);

            sb.Append("</w-frame>");
        }

        public override void RenderStyle(StringBuilder sb)
        {
            sb.Append("display:grid;");

            RenderBackground(sb);
            RenderPosition(sb);
            RenderTransform(sb);
            RenderBorder(sb);

            Parent.RenderChildLayout(this, sb);
        }

        public override void RenderChildLayout(WView child, StringBuilder sb)
        {
            sb.Append("justify-self:");
            switch (child.HorizontalOptions.Alignment)
            {
                case LayoutAlignment.Fill:
                    if (child.WidthRequest == -1)
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

            sb.Append("align-self:");
            switch (child.VerticalOptions.Alignment)
            {
                case LayoutAlignment.Fill:
                    if (child.HeightRequest == -1)
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
        }
    }
}
