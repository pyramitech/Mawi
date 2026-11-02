using Mawi.Views;
using static Mawi.Gallery.Ui;

namespace Mawi.Gallery.Pages
{
    internal static class TextPage
    {
        private const string LoremShort = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam.";

        public static WView Build()
        {
            var fonts = Card("Fonts",
                new WLabel { Text = "Segoe UI — the default set through WRoot.DefaultStyle", TextColor = Ink, FontSize = 15 },
                new WLabel { Text = "Georgia, for when it needs to look serious", TextColor = Ink, FontSize = 15, FontFamily = "Georgia" },
                new WLabel { Text = "Consolas, monospace for code", TextColor = Ink, FontSize = 15, FontFamily = "Consolas" },
                new WLabel { Text = "Weight 300", TextColor = Ink, FontSize = 15, FontWeight = 300 },
                new WLabel { Text = "Weight 700", TextColor = Ink, FontSize = 15, FontWeight = 700 });

            var decorated = Card("Decorations & transforms",
                new WLabel
                {
                    Text = "Wavy underline, straight from CSS",
                    TextColor = Ink,
                    FontSize = 15,
                    TextDecoration = new TextDecoration { Line = TextDecorationLine.Underline, Style = TextDecorationStyle.Wavy, Color = Pink },
                },
                new WLabel
                {
                    Text = "struck through",
                    TextColor = InkSoft,
                    FontSize = 15,
                    TextDecoration = new TextDecoration { Line = TextDecorationLine.LineThrough },
                },
                new WLabel { Text = "shouted at the renderer", TextColor = Ink, FontSize = 15, TextTransform = TextTransform.Uppercase, CharacterSpacing = 2 });

            var formattedLabel = new WLabel { FontSize = 15 };
            var formatted = new WFormattedString();
            formatted.Spans.Add(new WSpan { Text = "One label, ", TextColor = Ink });
            formatted.Spans.Add(new WSpan { Text = "many spans", TextColor = Accent, FontWeight = 700 });
            formatted.Spans.Add(new WSpan { Text = " with their own ", TextColor = Ink });
            formatted.Spans.Add(new WSpan { Text = "backgrounds", TextColor = Colors.White, BackgroundColor = Green });
            formatted.Spans.Add(new WSpan { Text = " and ", TextColor = Ink });
            formatted.Spans.Add(new WSpan { Text = "styles", TextColor = Violet, FontStyle = "italic" });
            formattedLabel.FormattedText = formatted;

            var wrapping = Card("Wrapping & truncation",
                Body("The labels below share the same long text but disagree on what to do with it:"),
                new WLabel { Text = LoremShort, TextColor = Ink, FontSize = 14, LineBreakMode = LineBreakMode.WordWrap },
                new WLabel { Text = LoremShort, TextColor = Ink, FontSize = 14, LineBreakMode = LineBreakMode.TailTruncation },
                new WLabel { Text = LoremShort, TextColor = Ink, FontSize = 14, MaxLines = 2, LineBreakMode = LineBreakMode.WordWrap },
                Body("And this one refuses to auto-wrap but honors its own \\n characters (PreserveLineBreaks):"),
                new WLabel
                {
                    Text = "no automatic wrapping on this line, however long it gets\nbut a manual line break\nstarts a new line",
                    TextColor = Ink,
                    FontSize = 14,
                    LineBreakMode = LineBreakMode.NoWrap,
                    PreserveLineBreaks = true,
                });

            return Page(PageTitle("Text & Fonts"), fonts, decorated, Card("Formatted strings", formattedLabel), wrapping);
        }
    }
}
