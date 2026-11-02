using Mawi.Fluent;
using Mawi.Views;

namespace Mawi.Gallery
{
    /// <summary>palette and the small building blocks every gallery page is made of</summary>
    internal static class Ui
    {
        public static readonly Color Accent = Color.FromArgb("#5B8DEF");
        public static readonly Color Violet = Color.FromArgb("#7C5BEF");
        public static readonly Color Pink = Color.FromArgb("#EF5B8D");
        public static readonly Color Green = Color.FromArgb("#3BAF7A");
        public static readonly Color Orange = Color.FromArgb("#EFA33B");
        public static readonly Color Sky = Color.FromArgb("#38BDF8");

        public static readonly Color SidebarBg = Color.FromArgb("#171C26");
        public static readonly Color SidebarText = Color.FromArgb("#C7CEDC");
        public static readonly Color ContentBg = Color.FromArgb("#F5F7FA");
        public static readonly Color CardBg = Colors.White;
        public static readonly Color FieldBg = Color.FromArgb("#EEF1F6");
        public static readonly Color Ink = Color.FromArgb("#1D2433");
        public static readonly Color InkSoft = Color.FromArgb("#5A6478");

        /// <summary>vertically scrolling page with the standard padding and section gap</summary>
        public static WScrollView Page(params WView[] sections)
        {
            var stack = new WStackLayout { Spacing = 18 }.Padding(28, 24);
            foreach (var section in sections)
            {
                stack.Children.Add(section);
            }
            return new WScrollView { Content = stack, Orientation = ScrollOrientation.Vertical }.Fill();
        }

        public static WLabel PageTitle(string text) => new WLabel
        {
            Text = text,
            TextColor = Ink,
            FontSize = 26,
            FontWeight = 700,
        };

        public static WLabel Body(string text) => new WLabel
        {
            Text = text,
            TextColor = InkSoft,
            FontSize = 14,
            LineBreakMode = LineBreakMode.WordWrap,
        };

        public static WStackLayout Card(string? heading, params WView[] children)
        {
            var stack = new WStackLayout { Spacing = 10 }
                .BackgroundColor(CardBg)
                .BorderRadius(12)
                .Padding(18, 16);

            if (heading != null)
            {
                stack.Children.Add(new WLabel { Text = heading, TextColor = Ink, FontSize = 17, FontWeight = 600 });
            }
            foreach (var child in children)
            {
                stack.Children.Add(child);
            }
            return stack;
        }

        public static WLabel Chip(string text, Color bg) => new WLabel
        {
            Text = text,
            TextColor = Colors.White,
            FontSize = 13,
        }.BackgroundColor(bg).BorderRadius(20).Padding(12, 6).Start();

        public static WButton PrimaryButton(string text) =>
            new WButton { Text = text, TextColor = Colors.White, FontSize = 14 }
                .BackgroundColor(Accent).BorderRadius(8).Padding(14, 9);

        public static WEntry Field(string placeholder) => new WEntry
        {
            Placeholder = placeholder,
            TextColor = Ink,
            PlaceholderColor = InkSoft,
            FontSize = 15,
        }.BackgroundColor(FieldBg).BorderRadius(8).Padding(12, 10);
    }
}
