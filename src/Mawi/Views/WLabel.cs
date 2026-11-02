using Mawi.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using PropertyChangingEventArgs = Microsoft.Maui.Controls.PropertyChangingEventArgs;

namespace Mawi.Views
{
    public class WLabel : WTextBase
    {
        private WFormattedString _formattedText = new WFormattedString();

        private bool _selectable = false;

        private string _text = null;

        private int _maxLines = -1;

        public bool Selectable { get => _selectable; set { _selectable = value; MarkUpdated(); } }
        public int MaxLines { get => _maxLines; set { _maxLines = value; MarkUpdated(); } }


        public WFormattedString FormattedText { get => _formattedText; set {
                if (this.Root != null)
                {
                    foreach (var span in _formattedText.Spans)
                    {
                        span.UnassignRoot();
                    }
                }
                _formattedText = value;
                _text = null;
                if (this.Root != null)
                {
                    foreach (var span in _formattedText.Spans)
                    {
                        span.AssignRoot(this.Root);
                    }
                }
                Invalidate(); } }

        //TODO: decide if I want normal text as text or turn into formatted text.
        //public string Text { get => _formattedText.ToString(); set {

        //        if (this.Root != null)
        //        {
        //            foreach (var span in _formattedText.Spans)
        //            {
        //                span.UnassignRoot();
        //            }
        //        }
        //        _formattedText = HttpUtility.HtmlEncode(value);
        //        if (this.Root != null)
        //        {
        //            foreach (var span in _formattedText.Spans)
        //            {
        //                span.AssignRoot(this.Root);
        //            }
        //        }
        //        Invalidate(); } }

        public string Text
        {
            get => _text; set
            {
                if (_formattedText != null)
                {
                    if (this.Root != null)
                    {
                        foreach (var span in _formattedText.Spans)
                        {
                            span.UnassignRoot();
                        }
                    }
                    _formattedText = null;
                }
                _text = value == null ? null : value.MakeHtmlSafe();
                Invalidate();
            }
        }

        public override void Render(StringBuilder sb)
        {
            this.UpdateState = UpdateState.None;

            sb.Append("<w-label");
            RenderInternals(sb);
            RenderEvents(sb);
            sb.Append(" style=\\\"");
            RenderStyle(sb);
            sb.Append("\\\"");
            sb.Append(">");

            if (_formattedText != null)
            {
                foreach (var span in _formattedText.Spans)
                {
                    span.Render(sb);
                }
            }
            else if (_text != null)
            {
                sb.Append(_text);
            }
            sb.Append("</w-label>");
        }

        public override void RenderChildLayout(WView child, StringBuilder sb)
        {
        }

        public override void RenderStyle(StringBuilder sb)
        {
            RenderBackground(sb);
            RenderPosition(sb);
            RenderTransform(sb);
            RenderFont(sb);

            if (!_selectable)
            {
                sb.Append("user-select: none;");
            }

            if (_maxLines != -1)
            {
                sb.Append("display: -webkit-box;-webkit-box-orient: vertical;-webkit-line-clamp:")
                    .Append(_maxLines).Append(';');
                
            }

            Parent.RenderChildLayout(this, sb);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (_formattedText != null)
            {
                SetInheritedBindingContext(_formattedText, BindingContext);
            }
        }

        internal override void AssignRoot(WRoot root)
        {
            base.AssignRoot(root);
            if (_formattedText != null)
            {
                foreach (var span in _formattedText.Spans)
                {
                    span.AssignRoot(root);
                }
            }
        }

        internal override void UnassignRoot()
        {
            base.UnassignRoot();
            if (_formattedText != null)
            {
                foreach (var span in _formattedText.Spans)
                {
                    span.UnassignRoot();
                }
            }
        }
    }

    public enum TextDecorationLine
    {
        None,
        Underline,
        Overline,
        LineThrough,
        Blink,
        SpellingError,
        GrammarError
    }

    public enum TextDecorationStyle
    {
        Solid,
        Double,
        Dotted,
        Dashed,
        Wavy
    }


    public sealed class TextDecoration
    {
        public WLength Thickness;
        public Color Color;
        public TextDecorationLine Line;
        public TextDecorationStyle Style;

        public void Write(StringBuilder sb)
        {
            if (Thickness.Value != 0)
            {
                sb.Append("text-decoration-thickness:");
                Thickness.Write(sb);
                sb.Append(';');
            }
            if (Line != default)
            {
                sb.Append("text-decoration-line:");
                sb.Append(Line switch
                {
                    TextDecorationLine.None => "none",
                    TextDecorationLine.Underline => "underline",
                    TextDecorationLine.Overline => "overline",
                    TextDecorationLine.LineThrough => "line-through",
                    TextDecorationLine.Blink => "blink",
                    TextDecorationLine.SpellingError => "spelling-error",
                    TextDecorationLine.GrammarError => "grammar-error",
                    _ => "none"
                });
                sb.Append(';');
            }
            if (Color != null)
            {
                sb.Append("text-decoration-color:");
                sb.Append(Color.ToRgbaHex(true));
                sb.Append(';');
            }
            if (Style != default)
            {
                sb.Append("text-decoration-style:");
                sb.Append(Style switch
                {
                    TextDecorationStyle.Solid => "solid",
                    TextDecorationStyle.Double => "double",
                    TextDecorationStyle.Dotted => "dotted",
                    TextDecorationStyle.Dashed => "dashed",
                    TextDecorationStyle.Wavy => "wavy",
                    _ => "solid"
                });
                sb.Append(';');

            }
        }
    }

    public abstract class WTextBase : WView
    {
        private TextAlignment _horizontalTextAlignment;
        private TextAlignment _verticalTextAlignment;
        private WLength _characterSpacing;

        private string _fontFamily;
        private WLength _fontSize = -1;
        private string _fontStyle;
        private short _fontWeight = -1;

        private Color _textColor;
        private TextTransform _textTransform = TextTransform.Default;

        private LineBreakMode _lineBreakMode = LineBreakMode.NoWrap;
        private bool _preserveLineBreaks;

        private TextDecoration _textDecoration = null;

        public TextAlignment HorizontalTextAlignment { get => _horizontalTextAlignment; set { _horizontalTextAlignment = value; Invalidate(); } }
        public TextAlignment VerticalTextAlignment { get => _verticalTextAlignment; set { _verticalTextAlignment = value; Invalidate(); } }

        public WLength CharacterSpacing { get => _characterSpacing; set { _characterSpacing = value; Invalidate(); } }
        public short FontWeight { get => _fontWeight; set { _fontWeight = value; Invalidate(); } }
        public string FontStyle { get => _fontStyle; set { _fontStyle = value; Invalidate(); } }

        public string FontFamily { get => _fontFamily; set { _fontFamily = value; Invalidate(); } }
        public WLength FontSize { get => _fontSize; set { _fontSize = value; Invalidate(); } }

        public TextTransform TextTransform { get => _textTransform; set { _textTransform = value; Invalidate(); } }

        [CssName("color")]
        public Color TextColor { get => _textColor; set { _textColor = value; Invalidate(); } }

        public LineBreakMode LineBreakMode { get => _lineBreakMode; set { _lineBreakMode = value; Invalidate(); } }

        /// <summary>
        /// makes \n in the text start a new line (and preserves consecutive spaces/tabs) without
        /// changing the wrapping behavior: NoWrap keeps refusing to auto-wrap, the wrap modes
        /// keep wrapping. ignored by the truncation modes, which are single-line by definition
        /// </summary>
        public bool PreserveLineBreaks { get => _preserveLineBreaks; set { _preserveLineBreaks = value; Invalidate(); } }

        public TextDecoration TextDecoration { get => _textDecoration; set { _textDecoration = value; Invalidate(); } }

        protected void RenderFont(StringBuilder sb, bool useFlex = false)
        {
            if (useFlex)
            {
                if (this.HorizontalTextAlignment != TextAlignment.Start)
                {
                    sb.Append("justify-content:");

                    switch (this.HorizontalTextAlignment)
                    {
                        case TextAlignment.Center:
                            sb.Append("center");
                            break;

                        case TextAlignment.End:
                            sb.Append("end");
                            break;

                        case TextAlignment.Justify:
                            sb.Append("space-between");
                            break;
                    }
                    sb.Append(';');
                }

                if (this.VerticalTextAlignment != TextAlignment.Start)
                {
                    //sb.Append("display:flex;flex-wrap:wrap;flex-direction: row;align-content:");
                    sb.Append("align-items:");

                    switch (this.VerticalTextAlignment)
                    {
                        case TextAlignment.Center:
                            sb.Append("center");
                            break;

                        case TextAlignment.End:
                            sb.Append("end");
                            break;

                        case TextAlignment.Justify:
                            sb.Append("space-between");
                            break;
                    }
                    sb.Append(';');
                }
            }
            else 
            { 
                if (this.HorizontalTextAlignment != TextAlignment.Start)
                {
                    sb.Append("text-align:");

                    switch (this.HorizontalTextAlignment)
                    {
                        case TextAlignment.Center:
                            sb.Append("center");
                            break;

                        case TextAlignment.End:
                            sb.Append("end");
                            break;

                        case TextAlignment.Justify:
                            sb.Append("justify");
                            break;
                    }
                    sb.Append(';');
                }

                if (this.VerticalTextAlignment != TextAlignment.Start)
                {
                    //sb.Append("display:flex;flex-wrap:wrap;flex-direction: row;align-content:");
                    sb.Append("align-content:");

                    switch (this.VerticalTextAlignment)
                    {
                        case TextAlignment.Center:
                            sb.Append("center");
                            break;

                        case TextAlignment.End:
                            sb.Append("end");
                            break;

                        case TextAlignment.Justify:
                            sb.Append("space-between");
                            break;
                    }
                    sb.Append(';');
                }
            }

            if (CharacterSpacing.Value != -1)
            {
                sb.Append("letter-spacing:");
                CharacterSpacing.Write(sb);
                sb.Append(';');
            }

            if (_fontWeight != -1)
            {
                sb.Append("font-weight:").Append(_fontWeight).Append(';');
            }

            if (!string.IsNullOrEmpty(_fontStyle))
            {
                sb.Append("font-style:").Append(_fontStyle.MakeAttributeSafe()).Append(';');
            }

            if (!string.IsNullOrEmpty(_fontFamily))
            {
                sb.Append("font-family:").Append(_fontFamily.MakeAttributeSafe()).Append(';');
            }

            if (_fontSize.Value != -1)
            {
                sb.Append("font-size:");
                _fontSize.Write(sb);
                sb.Append(';');
            }

            if (_textColor != null)
            {
                sb.Append("color:").Append(_textColor.ToRgbaHex(true)).Append(';');
            }

            if (_textTransform != TextTransform.Default)
            {
                sb.Append("text-transform:");
                switch (_textTransform)
                {
                    case TextTransform.None:
                        sb.Append("none");
                        break;

                    case TextTransform.Uppercase:
                        sb.Append("uppercase");
                        break;

                    case TextTransform.Lowercase:
                        sb.Append("lowercase");
                        break;
                }
                sb.Append(';');
            }

            if (_textDecoration != null)
            {
                _textDecoration.Write(sb);
            }


            switch (_lineBreakMode)
            {
                case LineBreakMode.NoWrap:
                    //pre = nowrap + honored \n and whitespace
                    sb.Append(_preserveLineBreaks ? "white-space: pre;" : "white-space: nowrap;");
                    break;

                case LineBreakMode.WordWrap:
                    //pre-wrap = normal wrapping + honored \n and whitespace
                    sb.Append(_preserveLineBreaks ? "white-space: pre-wrap;" : "white-space: normal;");
                    sb.Append("overflow-wrap: break-word;");
                    sb.Append("word-break: break-word;");

                    break;

                case LineBreakMode.CharacterWrap:
                    sb.Append(_preserveLineBreaks ? "white-space: pre-wrap;" : "white-space: normal;");
                    sb.Append("word-break: break-all;");
                    break;

                case LineBreakMode.HeadTruncation:
                    sb.Append("white-space: nowrap;");
                    sb.Append("overflow: hidden;");
                    sb.Append("text-overflow: ellipsis;");
                    sb.Append("direction: rtl;");
                    sb.Append("text-align: left;");
                    break;

                case LineBreakMode.TailTruncation:
                    sb.Append("white-space: nowrap;");
                    sb.Append("overflow: hidden;");
                    sb.Append("text-overflow: ellipsis;");
                    break;

                default: //middle truncate does not work so idk just do nothing
                    break;
            }
        }
    }

    public class WFormattedString : BindableObject
    {

        public WView Parent { get; internal set; }

        readonly WSpanCollection _spans = new WSpanCollection();
        internal event NotifyCollectionChangedEventHandler SpansCollectionChanged;

        public WFormattedString() => _spans.CollectionChanged += OnCollectionChanged;

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            for (int i = 0; i < Spans.Count; i++)
                SetInheritedBindingContext(Spans[i], BindingContext);
        }
        public IList<WSpan> Spans => _spans;

        public static explicit operator string(WFormattedString formatted) => formatted.ToString();

        public static implicit operator WFormattedString(string text) => new WFormattedString { Spans = { new WSpan { Text = text } } };

        public override string ToString() => string.Concat(Spans.Select(span => span.Text));

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Parent != null)
            {
                Parent.MarkDirty(UpdateState.Dirty);

                if (Parent.Root != null)
                {
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            (item as WView).UnassignRoot();
                        }
                    }
                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            var view = (item as WView);
                            view.AssignRoot(Parent.Root);
                        }
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (object item in e.OldItems)
                {
                    var bo = item as Span;
                    if (bo != null)
                    {
                        bo.PropertyChanging -= OnItemPropertyChanging;
                        bo.PropertyChanged -= OnItemPropertyChanged;
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (object item in e.NewItems)
                {
                    var bo = item as Span;
                    if (bo != null)
                    {
                        bo.PropertyChanging += OnItemPropertyChanging;
                        bo.PropertyChanged += OnItemPropertyChanged;
                    }

                }
            }

            OnPropertyChanged(nameof(Spans));
            SpansCollectionChanged?.Invoke(sender, e);
        }

        void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(Spans));

        void OnItemPropertyChanging(object sender, PropertyChangingEventArgs e) => OnPropertyChanging(nameof(Spans));

        class WSpanCollection : ObservableCollection<WSpan>
        {
            protected override void InsertItem(int index, WSpan item) => base.InsertItem(index, item ?? throw new ArgumentNullException(nameof(item)));
            protected override void SetItem(int index, WSpan item) => base.SetItem(index, item ?? throw new ArgumentNullException(nameof(item)));

            protected override void ClearItems()
            {
                var removed = new List<WSpan>(this);
                base.ClearItems();
                base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
            }
        }
        private sealed class WFormattedStringConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                => sourceType == typeof(string);

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
                => destinationType == typeof(string);

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is string strValue)
                {
                    return (FormattedString)strValue;
                }

                throw new NotSupportedException();
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (value is FormattedString formattedStr)
                {
                    return (string)formattedStr;
                }

                throw new NotSupportedException();
            }
        }
    }


    public class WSpan : WElement, IWRenderTarget
    {
        private TextAlignment _horizontalTextAlignment;
        private TextAlignment _verticalTextAlignment;
        private WLength _characterSpacing;

        private string _fontFamily;
        private WLength _fontSize = -1;
        private string _fontStyle;
        private short _fontWeight = -1;

        private Color _textColor;
        private TextTransform _textTransform = TextTransform.Default;

        private string _text = string.Empty;


        public TextAlignment HorizontalTextAlignment { get => _horizontalTextAlignment; set { _horizontalTextAlignment = value; MarkUpdated(); } }
        public TextAlignment VerticalTextAlignment { get => _verticalTextAlignment; set { _verticalTextAlignment = value; MarkUpdated(); } }

        public WLength CharacterSpacing { get => _characterSpacing; set { _characterSpacing = value; MarkUpdated(); } }
        public short FontWeight { get => _fontWeight; set { _fontWeight = value; MarkUpdated(); } }
        public string FontStyle { get => _fontStyle; set { _fontStyle = value; MarkUpdated(); } }

        public string FontFamily { get => _fontFamily; set { _fontFamily = value; MarkUpdated(); } }
        public WLength FontSize { get => _fontSize; set { _fontSize = value; MarkUpdated(); } }

        public TextTransform TextTransform { get => _textTransform; set { _textTransform = value; MarkUpdated(); } }
        public Color TextColor { get => _textColor; set { _textColor = value; MarkUpdated(); } }

        public string Text { get => _text; set { _text = value == null ? null : value.MakeHtmlSafe(); MarkUpdated(); } }

        private Color _backgroundColor = Colors.Transparent;


        [CssName("backgroundColor")]
        public Color BackgroundColor
        {
            get => _backgroundColor; set
            {
                _backgroundColor = value;
                MarkUpdated();
            }
        }


       // internal UpdateState UpdateState = UpdateState.New;

        public new WView Parent { get; internal set; }

        private void MarkUpdated()
        {
            if (Parent != null)
            {
                Parent.Invalidate();
            }
        }
        public void RenderStyle(StringBuilder sb)
        {
            sb.Append("white-space: pre-wrap;");
            if (BackgroundColor != null)
            {
                sb.Append("background-color:").Append(BackgroundColor.ToRgbaHex(true)).Append(';');
            }

            if (this.HorizontalTextAlignment != TextAlignment.Start)
            {
                sb.Append("text-align:");

                switch (this.HorizontalTextAlignment)
                {
                    case TextAlignment.Center:
                        sb.Append("center");
                        break;

                    case TextAlignment.End:
                        sb.Append("end");
                        break;

                    case TextAlignment.Justify:
                        sb.Append("justify");
                        break;
                }
                sb.Append(';');
            }

            if (this.VerticalTextAlignment != TextAlignment.Start)
            {
                sb.Append("display:flex;flex-direction: column;justify-content:");

                switch (this.HorizontalTextAlignment)
                {
                    case TextAlignment.Center:
                        sb.Append("center");
                        break;

                    case TextAlignment.End:
                        sb.Append("end");
                        break;

                    case TextAlignment.Justify:
                        sb.Append("space-between");
                        break;
                }
                sb.Append(';');
            }

            if (CharacterSpacing.Value != -1)
            {
                sb.Append("letter-spacing:");
                CharacterSpacing.Write(sb);
                sb.Append(';');
            }

            if (_fontWeight != -1)
            {
                sb.Append("font-weight:").Append(_fontWeight).Append(';');
            }

            if (!string.IsNullOrEmpty(_fontStyle))
            {
                sb.Append("font-style:").Append(_fontStyle.MakeAttributeSafe()).Append(';');
            }

            if (!string.IsNullOrEmpty(_fontFamily))
            {
                sb.Append("font-family:").Append(_fontFamily.MakeAttributeSafe()).Append(';');
            }

            if (_fontSize.Value != -1)
            {
                sb.Append("font-size:");
                _fontSize.Write(sb);
                sb.Append(';');
            }

            if (_textColor != null)
            {
                sb.Append("color:").Append(_textColor.ToRgbaHex(true)).Append(';');
            }

            if (_textTransform != TextTransform.Default)
            {
                sb.Append("text-transform:");
                switch (_textTransform)
                {
                    case TextTransform.None:
                        sb.Append("none");
                        break;

                    case TextTransform.Uppercase:
                        sb.Append("uppercase");
                        break;

                    case TextTransform.Lowercase:
                        sb.Append("lowercase");
                        break;
                }
                sb.Append(';');
            }
        }

        public void Render(StringBuilder sb)
        {
            sb.Append("<span id=\\\"w").Append(this.Id).Append("\\\" ");
            RenderEvents(sb);
            sb.Append(" style=\\\"");
            RenderStyle(sb);
            sb.Append("\\\">");
            sb.Append(this.Text);
            sb.Append("</span>");
        }

        public void RenderChildLayout(WView child, StringBuilder sb)
        {
            //doesn't have children
        }

        internal override void AssignRoot(WRoot root)
        {
            this.Root = root;
            root.Views[this.Id] = this;
        }

        internal override void UnassignRoot()
        {
            ObserveSize = false;
            this.Root.Views.TryRemove(this.Id, out _);
            this.Root = null;
        }
    }


}
