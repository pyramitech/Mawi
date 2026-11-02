using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Views
{
    public class WActivityIndicator : WView
    {
        private bool _isRunning;
        private Color _color;

        public bool IsRunning { get => _isRunning; set { _isRunning = value; Invalidate(); } }
        public Color Color { get => _color; set { _color = value; Invalidate(); } }

        public override void Render(StringBuilder sb)
        {
            this.UpdateState = UpdateState.None;

            sb.Append("<w-activity");
            RenderInternals(sb);
            RenderEvents(sb);
            sb.Append(" style=\\\"");
            RenderStyle(sb);
            sb.Append("\\\">");
            if (IsRunning)
            {
                sb.Append("<svg stroke=\\\"");
                sb.Append(_color.ToRgbaHex(true));
                sb.Append("\\\" viewBox=\\\"0 0 24 24\\\" xmlns=\\\"http://www.w3.org/2000/svg\\\"><style>.spinner_V8m1{transform-origin:center;animation:spinner_zKoa 2s linear infinite}.spinner_V8m1 circle{stroke-linecap:round;animation:spinner_YpZS 1.5s ease-in-out infinite}@keyframes spinner_zKoa{100%{transform:rotate(360deg)}}@keyframes spinner_YpZS{0%{stroke-dasharray:0 150;stroke-dashoffset:0}47.5%{stroke-dasharray:42 150;stroke-dashoffset:-16}95%,100%{stroke-dasharray:42 150;stroke-dashoffset:-59}}</style><g class=\\\"spinner_V8m1\\\"><circle cx=\\\"12\\\" cy=\\\"12\\\" r=\\\"9.5\\\" fill=\\\"none\\\" stroke-width=\\\"3\\\"></circle></g></svg>");
            }
            sb.Append("</w-activity>");
        }

        public override void RenderChildLayout(WView child, StringBuilder sb)
        {
            throw new NotImplementedException();
        }

        public override void RenderStyle(StringBuilder sb)
        {
            RenderBackground(sb);
            RenderPosition(sb);
            RenderTransform(sb);
            RenderBorder(sb);

            Parent.RenderChildLayout(this, sb);
        }
    }
}
