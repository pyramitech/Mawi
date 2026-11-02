using Mawi.Animation;
using Mawi.Fluent;
using Mawi.Views;
using Microsoft.Maui.Layouts;
using static Mawi.Gallery.Ui;

namespace Mawi.Gallery.Pages
{
    internal static class AnimationsPage
    {
        public static WView Build()
        {
            var box = new WBoxView()
                .Size(110)
                .BorderRadius(16)
                .Background(new LinearGradientBrush(
                    new GradientStopCollection
                    {
                        new GradientStop(Accent, 0),
                        new GradientStop(Pink, 1),
                    },
                    new Point(0, 0), new Point(1, 1)))
                .Start();

            bool moved = false, faded = false;
            double rotation = 0;

            var buttons = new WFlexLayout { Direction = FlexDirection.Row, Wrap = FlexWrap.Wrap };
            void Add(string text, Action click)
            {
                var b = PrimaryButton(text);
                b.Click += (s, e) => click();
                buttons.Children.Add(b.Margin(new WThickness(0, 0, 8, 8)));
            }

            Add("Translate", () => { box.TranslateTo(moved ? 0 : 260, 0, 450, "ease-in-out"); moved = !moved; });
            Add("Rotate", () => { rotation += 90; box.RotateTo(rotation, 400, "ease-in-out"); });
            Add("Scale", () => { box.ScaleTo(box.Scale.X > 1 ? 1 : 1.35, 300); });
            Add("Fade", () => { box.FadeTo(faded ? 1 : 0.25, 350); faded = !faded; });

            var status = new WLabel { Text = "", TextColor = InkSoft, FontSize = 13 };
            async void RunCombo()
            {
                status.Text = "running…";
                var anim = new WAnimation { Duration = 900, Fill = FillMode.Both, Easing = "ease-in-out" };
                anim.Keyframes.Add(new WKeyframe()
                    .AddProperty<WView, WPoint>(x => x.Translation, new WPoint(0, 0))
                    .AddProperty<WView>(x => x.Rotation, 0d));
                anim.Keyframes.Add(new WKeyframe().Offset(0.5)
                    .AddProperty<WView, WPoint>(x => x.Translation, new WPoint(200, 40))
                    .AddProperty<WView>(x => x.Rotation, 180d));
                anim.Keyframes.Add(new WKeyframe()
                    .AddProperty<WView, WPoint>(x => x.Translation, new WPoint(0, 0))
                    .AddProperty<WView>(x => x.Rotation, 360d));
                await box.AnimateAsync(anim);
                status.Text = "keyframe animation completed (awaited in C#)";
            }
            Add("Keyframe combo", RunCombo);

            //ctrl+wheel over the box zooms it (chromium reports trackpad pinch the same way);
            //the page's own zoom is suppressed by the wheel handler
            box.Wheel += (s, e) =>
            {
                if (!e.CtrlKey)
                    return;
                var factor = e.DeltaY < 0 ? 1.1 : 1 / 1.1;
                var scale = Math.Clamp(box.Scale.X * factor, 0.4, 2.5);
                box.Scale = new Point3(scale, scale, 1);
                status.Text = $"wheel zoom: {scale:P0}";
            };

            return Page(
                PageTitle("Animations"),
                Card("Web Animations API from C#",
                    Body("Each button drives the box through the browser's animation engine, the last one awaits completion back in C#. Ctrl+wheel over the box zooms it."),
                    box,
                    buttons,
                    status));
        }
    }
}
