using Mawi.Fluent;
using Mawi.Views;
using Microsoft.Maui.Layouts;
using static Mawi.Gallery.Ui;

namespace Mawi.Gallery.Pages
{
    internal static class LayoutsPage
    {
        public static WView Build()
        {
            //stack
            var hstack = new WStackLayout { Orientation = WStackOrientation.Horizontal, Spacing = 8 };
            hstack.Children.Add(Chip("horizontal", Accent));
            hstack.Children.Add(Chip("stack", Violet));
            hstack.Children.Add(Chip("with", Pink));
            hstack.Children.Add(Chip("spacing", Green));

            //grid
            var grid = new WGrid
            {
                RowDefinition = "60,60",
                ColumnDefinition = "*,*,*",
                RowSpacing = 8,
                ColumnSpacing = 8,
            };
            void Cell(int r, int c, Color color, string text, int colSpan = 1)
            {
                grid.Children.Add(new WLabel
                {
                    Text = text,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                }
                .BackgroundColor(color).BorderRadius(8)
                .Apply(v => v.LayoutSettings = new WGridLayoutSettings { Row = r, Column = c, ColumnSpan = colSpan }));
            }
            Cell(0, 0, Accent, "0,0");
            Cell(0, 1, Violet, "0,1 span 2", 2);
            Cell(1, 0, Green, "1,0");
            Cell(1, 1, Orange, "1,1");
            Cell(1, 2, Pink, "1,2");

            //flex wrap
            var flex = new WFlexLayout
            {
                Direction = FlexDirection.Row,
                Wrap = FlexWrap.Wrap,
                JustifyContent = FlexJustify.Start,
            };
            var flexColors = new[] { Accent, Violet, Pink, Orange, Green, Sky };
            for (int i = 0; i < 14; i++)
            {
                flex.Children.Add(Chip($"flex {i}", flexColors[i % flexColors.Length]).Margin(new WThickness(0, 0, 8, 8)));
            }

            //frame
            var frame = new WFrame
            {
                Content = Chip("centered by WFrame", Accent).Center(),
            }.Height(90).FillHorizontal().BackgroundColor(FieldBg).BorderRadius(8);

            return Page(
                PageTitle("Layouts"),
                Card("WStackLayout", Body("Horizontal orientation with an 8px gap."), hstack),
                Card("WGrid", Body("Two rows, three columns, spans, all rendered as CSS grid."), grid),
                Card("WFrame", Body("Single-child wrapper with the full alignment model, cheaper than a grid."), frame),
                Card("WFlexLayout", Body("Row direction with wrapping."), flex));
        }
    }
}
