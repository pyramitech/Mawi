using Mawi.Helpers;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Views
{
    public class WImage : WView, IImageContainer
    {
        private Aspect _aspect;
        protected ImageSource _source;


        public Aspect Aspect { get => _aspect; set { _aspect = value; MarkUpdated(); } }
        public ImageSource Source { get => _source; set { 
                _source = value;
                if (_source != null)
                {
                    if (Root != null)
                    {
                        LoadImage();
                    }
                }
                else
                {
                    SourceUrl = null;
                    Invalidate();
                }
            } 
        }

        internal string SourceUrl;
        string IImageContainer.SourceUrl { get => SourceUrl; set { SourceUrl = value; Invalidate(); } }

        public override void Render(StringBuilder sb)
        {
            this.UpdateState = UpdateState.None;

            sb.Append("<img");
            RenderInternals(sb);
            RenderEvents(sb);
            sb.Append(" style=\\\"");
            RenderStyle(sb);
            sb.Append("\\\"");
            if (!string.IsNullOrEmpty(SourceUrl))
            {
                sb.Append(" src=\\\"");
                sb.Append(SourceUrl);
                sb.Append("\\\"");
            }
            sb.Append("/>");
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

            sb.Append("object-fit:");
            switch (Aspect)
            {
                case Aspect.AspectFit:
                    sb.Append("contain");
                    break;

                case Aspect.AspectFill:
                    sb.Append("cover");
                    break;

                case Aspect.Fill:
                    sb.Append("fill");
                    break;

                case Aspect.Center:
                    sb.Append("none;");
                    sb.Append("object-position: center");
                    break;
            }
            sb.Append(';');

            Parent.RenderChildLayout(this, sb);
        }

        internal override void AssignRoot(WRoot root)
        {
            base.AssignRoot(root);
            if (_source != null && string.IsNullOrEmpty(SourceUrl))
            {
                LoadImage();
            }
        }


        void IImageContainer.LoadImage() => LoadImage();
        private void LoadImage()
        {
            ImageHelpers.LoadImage(this, Root, _source);
        }
    }

    internal interface IImageContainer
    {
        string SourceUrl { get; set; }

        void LoadImage();        
    }
}
