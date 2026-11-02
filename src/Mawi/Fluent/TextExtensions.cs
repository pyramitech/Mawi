using Mawi.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Fluent
{
    public static class TextExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView FontSize<TView>(this TView view, WLength value) where TView : WTextBase
        {
            view.FontSize = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView FontWeight<TView>(this TView view, short value) where TView : WTextBase
        {
            view.FontWeight = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Bold<TView>(this TView view) where TView : WTextBase
        {
            view.FontWeight = 700;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Italic<TView>(this TView view) where TView : WTextBase
        {
            view.FontStyle = "italic";
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Font<TView>(this TView view, string family, WLength size) where TView : WTextBase
        {
            view.FontFamily = family;
            view.FontSize = size;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Font<TView>(this TView view, string family, WLength size, bool bold, bool italic) where TView : WTextBase
        {
            view.FontFamily = family;
            view.FontSize = size;
            view.FontWeight = (short)(bold ? 700 : 400);
            view.FontStyle = italic ? "italic" : "normal";
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView TextColor<TView>(this TView view, Color color) where TView : WTextBase
        {
            view.TextColor = color;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView TextStart<TView>(this TView view) where TView : WTextBase
        {
            view.HorizontalTextAlignment = TextAlignment.Start;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView TextCenterHorizontal<TView>(this TView view) where TView : WTextBase
        {
            view.HorizontalTextAlignment = TextAlignment.Center;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView TextEnd<TView>(this TView view) where TView : WTextBase
        {
            view.HorizontalTextAlignment = TextAlignment.Start;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView TextJustify<TView>(this TView view) where TView : WTextBase
        {
            view.HorizontalTextAlignment = TextAlignment.Justify;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView TextTop<TView>(this TView view) where TView : WTextBase
        {
            view.VerticalTextAlignment = TextAlignment.Start;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView TextCenterVertical<TView>(this TView view) where TView : WTextBase
        {
            view.VerticalTextAlignment = TextAlignment.Center;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView TextBottom<TView>(this TView view) where TView : WTextBase
        {
            view.VerticalTextAlignment = TextAlignment.End;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView TextCenter<TView>(this TView view) where TView : WTextBase
        {
            view.HorizontalTextAlignment = TextAlignment.Center;
            view.VerticalTextAlignment = TextAlignment.Center;
            return view;
        }

    }

    public static class LabelExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Text<TView>(this TView view, string value) where TView : WLabel
        {
            view.Text = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView FormattedText<TView>(this TView view, WSpan[] value) where TView : WLabel
        {
            view.FormattedText.Spans.Clear();
            foreach (var span in value)
            {
                view.FormattedText.Spans.Add(span);
            }
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView MaxLines<TView>(this TView view, int maxLines) where TView : WLabel
        {
            view.MaxLines = maxLines;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Selectable<TView>(this TView view, bool selectable) where TView : WLabel
        {
            view.Selectable = selectable;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView PreserveLineBreaks<TView>(this TView view, bool preserve = true) where TView : WTextBase
        {
            view.PreserveLineBreaks = preserve;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView LineBreakMode<TView>(this TView view, LineBreakMode mode) where TView : WTextBase
        {
            view.LineBreakMode = mode;
            return view;
        }
    }

    public static class EntryExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Text<TView>(this TView view, string value) where TView : WEntry
        {
            view.Text = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Placeholder<TView>(this TView view, string value) where TView : WEntry
        {
            view.Placeholder = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView IsPassword<TView>(this TView view, bool value) where TView : WEntry
        {
            view.IsPassword = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView IsReadOnly<TView>(this TView view, bool value) where TView : WEntry
        {
            view.IsReadOnly = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView IsSpellCheckEnabled<TView>(this TView view, bool value) where TView : WEntry
        {
            view.IsSpellCheckEnabled = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView IsTextPredictionEnabled<TView>(this TView view, bool value) where TView : WEntry
        {
            view.IsTextPredictionEnabled = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView MaxLength<TView>(this TView view, int value) where TView : WEntry
        {
            view.MaxLength = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView PlaceholderColor<TView>(this TView view, Color color) where TView : WEntry
        {
            view.PlaceholderColor = color;
            return view;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Keyboard<TView>(this TView view, WKeyboard keyboard) where TView : WEntry
        {
            view.Keyboard = keyboard;
            return view;
        }
    }
}
