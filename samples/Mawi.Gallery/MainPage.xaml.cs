using Mawi.Animation;
using Mawi.Fluent;
using Mawi.Gallery.Pages;
using Mawi.Views;
using System.Globalization;

namespace Mawi.Gallery
{
    /// <summary>
    /// hosts the gallery: a sidebar plus a WShell whose routes are the individual pages
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private static readonly (string Route, string Title, Func<WView> Build)[] Routes =
        {
            ("welcome", "Welcome", WelcomePage.Build),
            ("layouts", "Layouts", LayoutsPage.Build),
            ("text", "Text & Fonts", TextPage.Build),
            ("inputs", "Inputs", InputsPage.Build),
            ("binding", "Data Binding", () => new BindingPage().Fill()),
            ("list", "Virtual List", VirtualListPage.Build),
            ("animations", "Animations", AnimationsPage.Build),
        };

        private readonly WShell _shell;
        private readonly Dictionary<string, WButton> _navButtons = new();
        private string _activeRoute = "";

        public MainPage()
        {
            //required by Mawi: numeric CSS values are formatted with the current culture
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            //must be set before the root initializes, it goes straight into the page's style tag
            WRoot.DefaultStyle = "body { font-family: 'Segoe UI', system-ui, sans-serif; }"
                + " [disabled] { opacity: 0.45; }"; //mawi doesn't auto-dim disabled views, the app decides

            InitializeComponent();

            _shell = CreateShell();
            var root = BuildRoot();
            //picked before the shell is attached, so the first page renders with the initial
            //batch instead of mounting with an animation
            Navigate("welcome");
            RootView.Content = root;

            RootView.SetScrollbarTheme(new WScrollbarTheme
            {
                Thickness = 10,
                ThumbColor = Color.FromArgb("#B9C2D4"),
                ThumbHoverColor = Ui.Accent,
                TrackColor = Colors.Transparent,
                ThumbRadius = 5,
                ThumbMargin = 2,
            });
        }

        private static WShell CreateShell()
        {
            var shell = new WShell().Fill();

            //every page fades in and slides up a little when it mounts
            shell.DefaultMountAnimation = new WAnimation
            {
                Duration = 220,
                Fill = FillMode.Both,
                Easing = "ease-out",
            };
            shell.DefaultMountAnimation.Keyframes.Add(new WKeyframe()
                .AddProperty<WView>(x => x.Opacity, 0)
                .AddProperty<WView, WPoint>(x => x.Translation, new WPoint(0, 14)));
            shell.DefaultMountAnimation.Keyframes.Add(new WKeyframe()
                .AddProperty<WView>(x => x.Opacity, 1)
                .AddProperty<WView, WPoint>(x => x.Translation, new WPoint(0, 0)));

            foreach (var (route, _, build) in Routes)
            {
                shell.AddPage(route, build);
            }
            return shell;
        }

        private WView BuildRoot()
        {
            var root = new WGrid { ColumnDefinition = "230,*" }
                .Fill()
                .BackgroundColor(Ui.ContentBg);

            root.Children.Add(BuildSidebar());
            root.Children.Add(_shell.Apply(s => s.LayoutSettings = new WGridLayoutSettings { Column = 1 }));
            return root;
        }

        private WView BuildSidebar()
        {
            var stack = new WStackLayout { Spacing = 4 }
                .Fill()
                .BackgroundColor(Ui.SidebarBg)
                .Padding(14, 18)
                .Apply(s => s.LayoutSettings = new WGridLayoutSettings { Column = 0 });

            stack.Children.Add(new WLabel
            {
                Text = "Mawi",
                TextColor = Colors.White,
                FontSize = 30,
                FontWeight = 700,
            }.Margin(new WThickness(8, 0, 0, 2)));

            stack.Children.Add(new WLabel
            {
                Text = "gallery",
                TextColor = Ui.SidebarText,
                FontSize = 13,
            }.Margin(new WThickness(8, 0, 0, 22)));

            foreach (var (route, title, _) in Routes)
            {
                var btn = new WButton
                {
                    Text = title,
                    TextColor = Ui.SidebarText,
                    FontSize = 15,
                    HorizontalTextAlignment = TextAlignment.Start,
                }
                .FillHorizontal()
                .Padding(12, 10)
                .BorderRadius(8);

                btn.Click += (s, e) => Navigate(route);
                _navButtons[route] = btn;
                stack.Children.Add(btn);
            }

            return stack;
        }

        private void Navigate(string route)
        {
            if (route == _activeRoute)
                return;
            _activeRoute = route;

            foreach (var (r, btn) in _navButtons)
            {
                btn.BackgroundColor = r == route ? Ui.Accent : Colors.Transparent;
                btn.TextColor = r == route ? Colors.White : Ui.SidebarText;
            }

            _ = _shell.NavigateTo(route);
        }
    }
}
