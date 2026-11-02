using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Views
{
    public class WHtml : WView
    {
        private string _html = string.Empty;
        public string Html { get => _html; set { _html = value?.Replace("\"", "\\\""); Invalidate(); } }

        public override void Render(StringBuilder sb)
        {
            this.UpdateState = UpdateState.None;

            sb.Append("<w-html");
            RenderInternals(sb);
            RenderEvents(sb);
            sb.Append(" style=\\\"");
            RenderStyle(sb);
            sb.Append("\\\">");
            if (_html != null)
            {
                sb.Append(_html);
            }
            sb.Append("</w-html>");
        }

        public void RunScript(string script)
        {
            Root.Execute(script);
        }

        //will never have children
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
            sb.Append("position: relative;");
        }
    }
}
