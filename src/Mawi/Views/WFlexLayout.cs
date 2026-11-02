using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Views
{
    public class WFlexLayout : WContainer
    {
        private FlexDirection _direction;
        private FlexWrap _wrap;
        private FlexJustify _justify;
        private FlexAlignItems _alignItems;
        private FlexAlignContent _alignContent;

        public FlexDirection Direction { get => _direction; set { _direction = value; MarkUpdated(); } }
        public FlexWrap Wrap { get => _wrap; set { _wrap = value; MarkUpdated(); } }
        public FlexJustify JustifyContent { get => _justify; set { _justify = value; MarkUpdated(); } }
        public FlexAlignItems AlignItems { get => _alignItems; set { _alignItems = value; MarkUpdated(); } }
        public FlexAlignContent AlignContent { get => _alignContent; set { _alignContent = value; MarkUpdated(); } }


        public override void Render(StringBuilder sb)
        {
            this.UpdateState = UpdateState.None;

            sb.Append("<w-flex");
            RenderInternals(sb);
            RenderEvents(sb);
            sb.Append(" style=\\\"");
            RenderStyle(sb);

            sb.Append("\\\">");

            foreach (var child in Children)
            {
                var view = child;
                child.Render(sb);
            }
            sb.Append("</w-flex>");
        }

        public override void RenderChildLayout(WView child, StringBuilder sb)
        {
            var settings = child.LayoutSettings as WFlexLayoutSettings;
            if (settings == null)
            {
                //TODO: can't actually auto set, because it triggers update during render loop...
                return;
                //settings = new WFlexLayoutSettings();
                //child.LayoutSettings = settings;
            }

            if (settings.AlignSelf != FlexAlignSelf.Auto)
            {
                sb.Append("align-self: ");
                switch (settings.AlignSelf) {
                    case FlexAlignSelf.Start:
                        sb.Append("flex-start");
                        break;

                    case FlexAlignSelf.Center:
                        sb.Append("center");
                        break;

                    case FlexAlignSelf.End:
                        sb.Append("flex-end");
                        break;

                    case FlexAlignSelf.Stretch:
                        sb.Append("stretch");
                        break;
                }
                sb.Append(';');
            }

            sb.Append("flex: ").Append(settings.Grow).Append(' ').Append(settings.Shrink).Append(' ');
            var basis = settings.FlexBasis;
            if (basis.IsAuto)
            {
                sb.Append("auto");
            }
            else if (basis.IsRelative)
            {
                sb.Append(basis.Length * 100f).Append("%");
            }
            else
            {
                if (Math.Abs(basis.Length) <= float.Epsilon) //annoying float crap
                {
                    sb.Append("0");
                }
                else
                {
                    sb.Append(basis.Length).Append("px");
                }
            }
            sb.Append(';');

            if (settings.Order != 0)
            {
                sb.Append("order: ").Append(settings.Order).Append(';');
            }
        }

        public override void RenderStyle(StringBuilder sb)
        {
            sb.Append("display:flex;");

            RenderBackground(sb);
            RenderPosition(sb);
            RenderTransform(sb);
            RenderBorder(sb);

            if (_direction != FlexDirection.Row) //default
            {
                sb.Append("flex-direction: ");
                switch (_direction) {
                    case FlexDirection.RowReverse:
                        sb.Append("row-reverse");
                        break;

                    case FlexDirection.Column:
                        sb.Append("column");
                        break;

                    case FlexDirection.ColumnReverse:
                        sb.Append("column-reverse");
                        break;
                }
                sb.Append(';');
            }

            if (_wrap != FlexWrap.NoWrap)
            {
                sb.Append("flex-wrap: ");
                switch (_wrap)
                {
                    case FlexWrap.Wrap:
                        sb.Append("wrap");
                        break;

                    case FlexWrap.Reverse:
                        sb.Append("wrap-reverse");
                        break;
                }
                sb.Append(';');
            }

            if (_justify != FlexJustify.Start)
            {
                sb.Append("justify-content: ");
                switch (_justify)
                {
                    case FlexJustify.Center:
                        sb.Append("center");
                        break;

                    case FlexJustify.End:
                        sb.Append("end");
                        break;

                    case FlexJustify.SpaceBetween:
                        sb.Append("space-between");
                        break;

                    case FlexJustify.SpaceAround:
                        sb.Append("space-around");
                        break;

                    case FlexJustify.SpaceEvenly:
                        sb.Append("space-evenly");
                        break;
                }
                sb.Append(';');
            }

            if (_alignItems != FlexAlignItems.Stretch)
            {
                sb.Append("align-items: ");
                switch (_alignItems)
                {
                    case FlexAlignItems.Start:
                        sb.Append("start");
                        break;

                    case FlexAlignItems.Center:
                        sb.Append("center");
                        break;

                    case FlexAlignItems.End:
                        sb.Append("end");
                        break;
                }
                sb.Append(';');
            }

            if (_alignContent != FlexAlignContent.Stretch)
            {
                sb.Append("align-content: ");
                switch (_alignContent)
                {
                    case FlexAlignContent.Start:
                        sb.Append("start");
                        break;

                    case FlexAlignContent.Center:
                        sb.Append("center");
                        break;

                    case FlexAlignContent.End:
                        sb.Append("end");
                        break;

                    case FlexAlignContent.SpaceBetween:
                        sb.Append("space-between");
                        break;

                    case FlexAlignContent.SpaceAround:
                        sb.Append("space-around");
                        break;

                    case FlexAlignContent.SpaceEvenly:
                        sb.Append("space-evenly");
                        break;
                }
                sb.Append(';');
            }

            Parent.RenderChildLayout(this, sb);
        }
    }

    public class WFlexLayoutSettings : IWLayoutSettings
    {
        void IWLayoutSettings.AssignParent(WView parent)
        {
            _parent = parent;
        }

        private WView _parent;

        private FlexAlignSelf _alignSelf;
        private WFlexBasis _flexBasis;
        private float _grow;
        private float _shrink;
        private int _order;

        public FlexAlignSelf AlignSelf { get => _alignSelf; set { _alignSelf = value; _parent?.MarkUpdated(); } }
        public WFlexBasis FlexBasis { get => _flexBasis; set { _flexBasis = value; _parent?.MarkUpdated(); } }
        public float Grow { get => _grow; set { _grow = value; _parent?.MarkUpdated(); } }
        public float Shrink { get => _shrink; set { _shrink = value; _parent?.MarkUpdated(); } }
        public int Order { get => _order; set { _order = value; _parent?.MarkUpdated(); } }
    }
}
