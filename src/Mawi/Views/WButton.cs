using Mawi.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Maui.Controls.Button;

namespace Mawi.Views
{
    public class WButton : WTextBase, IImageContainer
    {
        protected ImageSource _source;
        internal string SourceUrl;
        string IImageContainer.SourceUrl { get => SourceUrl; set { SourceUrl = value; Invalidate(); } }

        private WButtonContentLayout _contentLayout = null;
        private string _text = null;

        public string Text { get => _text; set { _text = value == null ? null : value.MakeHtmlSafe(); Invalidate(); } }

        public WButtonContentLayout ContentLayout { get => _contentLayout; set { _contentLayout = value; MarkUpdated(); } }

        public ImageSource Source
        {
            get => _source; set
            {
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

        public WButton()
        {
            this.HorizontalTextAlignment = TextAlignment.Center;
            this.VerticalTextAlignment = TextAlignment.Center;
        }

        void IImageContainer.LoadImage() => LoadImage();
        private void LoadImage()
        {
            ImageHelpers.LoadImage(this, Root, _source);
        }

        public override void Render(StringBuilder sb)
        {
            this.UpdateState = UpdateState.None;

            sb.Append("<w-button");
            RenderInternals(sb);
            RenderEvents(sb);
            sb.Append(" style=\\\"");
            RenderStyle(sb);
            sb.Append("\\\"");
            sb.Append(">");

            if (!string.IsNullOrEmpty(SourceUrl))
            {
                sb.Append("<img style=\\\"object-fit:contain;\\\" src=\\\"");
                sb.Append(SourceUrl);
                sb.Append("\\\"/>");
            }

            if (!string.IsNullOrEmpty(this._text))
            {
                sb.Append("<span style=\\\"width:100%; display:flex;");
                RenderFont(sb, true);
                sb.Append("\\\">");
                sb.Append(this.Text);
                sb.Append("</span>");
            }
            sb.Append("</w-button>");
        }

        public override void RenderChildLayout(WView child, StringBuilder sb)
        {
            throw new NotImplementedException();
        }

        public override void RenderStyle(StringBuilder sb)
        {
            sb.Append("display:flex;");

            if (_contentLayout != null)
            {
                sb.Append("flex-direction: ");
                switch (_contentLayout.Position)
                {
                    case WButtonContentLayout.ImagePosition.Left:
                        sb.Append("row;");
                        sb.Append("column-gap:");
                        _contentLayout.Spacing.Write(sb);
                        break;

                    case WButtonContentLayout.ImagePosition.Top:
                        sb.Append("column;");
                        sb.Append("row-gap:");
                        _contentLayout.Spacing.Write(sb);
                        break;

                    case WButtonContentLayout.ImagePosition.Right:
                        sb.Append("row-reverse;");
                        sb.Append("column-gap:");
                        _contentLayout.Spacing.Write(sb);
                        break;

                    case WButtonContentLayout.ImagePosition.Bottom:
                        sb.Append("column-reverse;");
                        sb.Append("row-gap:");
                        _contentLayout.Spacing.Write(sb);
                        break;
                }
                sb.Append(";");
            }

            RenderBackground(sb);
            RenderPosition(sb);
            RenderTransform(sb);
            RenderBorder(sb);

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
    }

    /// <summary>
    /// ripped from MAUI
    /// </summary>
    [System.ComponentModel.TypeConverter(typeof(WButtonContentTypeConverter))]
    public sealed class WButtonContentLayout
    {
        /// <summary>
        /// Enumerates values that determine the position of the image on the button.
        /// </summary>
        public enum ImagePosition
        {
            Left,
            Top,
            Right,
            Bottom
        }

        /// <summary>
        /// Initializes a new instance of the this class.
        /// </summary>
        /// <param name="position">The position of the image.</param>
        /// <param name="spacing">The spacing for the button content.</param>
        public WButtonContentLayout(ImagePosition position, WLength spacing)
        {
            Position = position;
            Spacing = spacing;
        }

        /// <summary>
        /// Gets the position of the image on the button.
        /// </summary>
        public ImagePosition Position { get; }

        /// <summary>
        /// Gets the spacing for the button content.
        /// </summary>
        public WLength Spacing { get; }

        /// <summary>
        /// Gets the string representation of this object.
        /// </summary>
        /// <returns>Prints out the values of <see cref="Position"/> and <see cref="Spacing"/>.</returns>
        public override string ToString() => $"Image Position = {Position}, Spacing = {Spacing}";
    }

    /// <summary>
    /// ripped from MAUI
    /// </summary>
    public sealed class WButtonContentTypeConverter : TypeConverter
    {
        const double DefaultSpacing = 10;

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => sourceType == typeof(string);

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => false;

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            // IMPORTANT! Update ButtonContentDesignTypeConverter.IsValid if making changes here
            var strValue = value?.ToString();
            if (strValue == null)
                throw new InvalidOperationException($"Cannot convert null into {typeof(WButtonContentLayout)}");

            string[] parts = strValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 1 && parts.Length != 2)
                throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(WButtonContentLayout)}");

            WLength spacing;
            var position = WButtonContentLayout.ImagePosition.Left;

            var spacingFirst = char.IsDigit(parts[0][0]);

            int positionIndex = spacingFirst ? (parts.Length == 2 ? 1 : -1) : 0;
            int spacingIndex = spacingFirst ? 0 : (parts.Length == 2 ? 1 : -1);

            if (spacingIndex > -1)
                spacing = WLengthConverter.ParseFromString(parts[spacingIndex]);
            else spacing = new WLength(DefaultSpacing);

            if (positionIndex > -1)
                position = (WButtonContentLayout.ImagePosition)Enum.Parse(typeof(ButtonContentLayout.ImagePosition), parts[positionIndex], true);

            return new WButtonContentLayout(position, spacing);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            => throw new NotSupportedException();
    }
}
