using Mawi.Fluent;
using Mawi.Views;
using static Mawi.Gallery.Ui;

namespace Mawi.Gallery.Pages
{
    internal static class VirtualListPage
    {
        public static WView Build()
        {
            var list = new WVirtualList
            {
                EstimatedItemSize = 52,
                OverscanCount = 5,
            }.Fill().BackgroundColor(CardBg).BorderRadius(12);

            list.ItemTemplate = o =>
            {
                var item = (Item)o;
                var row = new WStackLayout { Orientation = WStackOrientation.Horizontal, Spacing = 12 }
                    .Padding(16, 10)
                    .BackgroundColor(item.Index % 2 == 0 ? Colors.White : Color.FromArgb("#F2F5FA"));

                row.Children.Add(new WLabel
                {
                    Text = item.Index.ToString(),
                    TextColor = Colors.White,
                    FontSize = 12,
                    HorizontalTextAlignment = TextAlignment.Center,
                }.BackgroundColor(Accent).BorderRadius(14).MinWidth(52).Padding(6, 4).Top());

                row.Children.Add(new WLabel
                {
                    Text = item.Text,
                    TextColor = Ink,
                    FontSize = 14,
                    LineBreakMode = LineBreakMode.WordWrap,
                }.FillHorizontal());

                return row;
            };
            list.ItemsSource = Item.Generate(100_000);

            var jumpEntry = new WEntry
            {
                Placeholder = "index",
                Keyboard = WKeyboard.Numeric,
                TextColor = Ink,
                PlaceholderColor = InkSoft,
                FontSize = 14,
            }.BackgroundColor(FieldBg).BorderRadius(8).Padding(12, 8).Width(120);

            var jumpBtn = PrimaryButton("Jump").Padding(16, 8);
            jumpBtn.Click += (s, e) =>
            {
                if (int.TryParse(jumpEntry.Text, out var index))
                {
                    list.ScrollToIndex(index);
                }
            };

            var header = new WStackLayout { Orientation = WStackOrientation.Horizontal, Spacing = 10 };
            header.Children.Add(new WLabel
            {
                Text = "100,000 items, variable heights, ~20 in the DOM",
                TextColor = InkSoft,
                FontSize = 14,
            }.CenterVertical());
            header.Children.Add(jumpEntry);
            header.Children.Add(jumpBtn);

            var page = new WGrid { RowDefinition = "auto,auto,*", RowSpacing = 14 }.Fill().Padding(28, 24);
            page.Children.Add(PageTitle("Virtual List").Apply(v => v.LayoutSettings = new WGridLayoutSettings { Row = 0 }).Start().Top());
            page.Children.Add(header.Apply(v => v.LayoutSettings = new WGridLayoutSettings { Row = 1 }).Start().Top());
            page.Children.Add(list.Apply(v => v.LayoutSettings = new WGridLayoutSettings { Row = 2 }));
            return page;
        }

        private sealed record Item(int Index, string Text)
        {
            public static List<object> Generate(int count)
            {
                var words = new[] { "lorem", "ipsum", "dolor", "sit", "amet", "consectetur", "adipiscing", "elit", "sed", "eiusmod" };
                var items = new List<object>(count);
                for (int i = 0; i < count; i++)
                {
                    int wordCount = 3 + (i * 7) % 24;
                    var text = string.Join(' ', Enumerable.Range(0, wordCount).Select(w => words[(i + w) % words.Length]));
                    items.Add(new Item(i, text));
                }
                return items;
            }
        }
    }
}
