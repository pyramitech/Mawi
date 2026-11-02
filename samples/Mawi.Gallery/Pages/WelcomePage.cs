using Mawi.Fluent;
using Mawi.Views;
using static Mawi.Gallery.Ui;

namespace Mawi.Gallery.Pages
{
    internal static class WelcomePage
    {
        public static WView Build()
        {
            var hero = new WStackLayout { Spacing = 8 }
                .Background(new LinearGradientBrush(
                    new GradientStopCollection
                    {
                        new GradientStop(Accent, 0),
                        new GradientStop(Violet, 1),
                    },
                    new Point(0, 0), new Point(1, 1)))
                .BorderRadius(14)
                .Padding(26, 24);

            hero.Children.Add(new WLabel { Text = "Mawi", TextColor = Colors.White, FontSize = 34, FontWeight = 700 });
            hero.Children.Add(new WLabel
            {
                Text = "The MAUI-style framework that renders into a WebView, because the DOM builds UI faster than native does.",
                TextColor = Color.FromArgb("#E8ECFF"),
                FontSize = 15,
                LineBreakMode = LineBreakMode.WordWrap,
            });

            var features = Card("What's inside",
                Body("• Layouts: stack, grid, flex and scroll views mapped straight onto CSS"),
                Body("• Full pointer, keyboard, focus, scroll and resize events round-tripped to C#"),
                Body("• {w:Bind} data binding on plain CLR properties, zero cost when unused"),
                Body("• Estimated-size list virtualization with anchored scrolling"),
                Body("• Web Animations API keyframes driven from C#"),
                Body("• Shell navigation with mount animations, this app runs on it"));

            var perf = Card("Why",
                Body("Constructing this entire page happens in one innerHTML batch, which is the exact " +
                     "fast path browsers are optimized for. On Android the difference against native " +
                     "element construction is more than tenfold."));

            return Page(PageTitle("Welcome"), hero, features, perf);
        }
    }
}
