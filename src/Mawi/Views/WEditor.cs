using Mawi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Views
{
    public class WEditor : WEntryBase
    {
        public WEditor()
        {

        }

        private bool _isPassword;

        public override void Render(StringBuilder sb)
        {
            this.UpdateState = UpdateState.None;

            sb.Append("<textarea");
            RenderInternals(sb);
            RenderEvents(sb);
            sb.Append(" style=\\\"");
            RenderStyle(sb);
            sb.Append("\\\"");

            //textarea has no value attribute, initial text is the element content
            RenderTextSettings(sb, false);

            sb.Append(">");
            //browsers drop a single newline right after the opening tag, emit one so the text's own leading newline survives
            sb.Append("\\n");
            if (!string.IsNullOrEmpty(Text))
            {
                sb.Append(Text.MakeAttributeSafe());
            }
            sb.Append("</textarea>");
        }
    }
}
