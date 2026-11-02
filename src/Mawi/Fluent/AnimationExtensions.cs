using Mawi.Animation;
using Mawi.Views;
using System.Runtime.CompilerServices;

namespace Mawi.Fluent
{
    public static class AnimationExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WKeyframe Offset(this WKeyframe frame, double offset)
        {
            frame.Offset = offset;
            return frame;
        }
    }

    public static class ViewAnimationExtensions
    {
        public static TView BackgroundColorTo<TView>(this TView view, Color color, uint length = 250, string easing = null) where TView : WView
        {
            var anim = new WAnimation();
            anim.Duration = length;
            anim.Fill = FillMode.Both;
            anim.Easing = easing;
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView>(x => x.BackgroundColor, view.BackgroundColor ?? Colors.Transparent));
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView>(x => x.BackgroundColor, color));
            view.Animate(anim);
            view.BackgroundColor = color;
            return view;
        }

        public static async Task<TView> BackgroundColorToAsync<TView>(this TView view, Color color, uint length = 250, string easing = null) where TView : WView
        {
            var anim = new WAnimation();
            anim.Duration = length;
            anim.Fill = FillMode.Both;
            anim.Easing = easing;
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView>(x => x.BackgroundColor, view.BackgroundColor ?? Colors.Transparent));
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView>(x => x.BackgroundColor, color));
            view.BackgroundColor = color;
            await view.AnimateAsync(anim);
            return view;
        }

        public static TView TextColorTo<TView>(this TView view, Color color, uint length = 250, string easing = null) where TView : WTextBase
        {
            var anim = new WAnimation();
            anim.Duration = length;
            anim.Fill = FillMode.Both;
            anim.Easing = easing;
            anim.Keyframes.Add(new WKeyframe().AddProperty<WTextBase>(x => x.TextColor, view.TextColor ?? Colors.Transparent));
            anim.Keyframes.Add(new WKeyframe().AddProperty<WTextBase>(x => x.TextColor, color));
            view.Animate(anim);
            view.TextColor = color;
            return view;
        }

        public static async Task<TView> TextColorToAsync<TView>(this TView view, Color color, uint length = 250, string easing = null) where TView : WTextBase
        {
            var anim = new WAnimation();
            anim.Duration = length;
            anim.Fill = FillMode.Both;
            anim.Easing = easing;
            anim.Keyframes.Add(new WKeyframe().AddProperty<WTextBase>(x => x.TextColor, view.TextColor ?? Colors.Transparent));
            anim.Keyframes.Add(new WKeyframe().AddProperty<WTextBase>(x => x.TextColor, color));
            view.TextColor = color;
            await view.AnimateAsync(anim);
            return view;
        }

        public static TView FadeTo<TView>(this TView view, double opacity, uint length = 250, string easing = null) where TView : WView
        {
            var anim = new WAnimation();
            anim.Duration = length;
            anim.Fill = FillMode.Both;
            anim.Easing = easing;
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView>(x => x.Opacity, view.Opacity));
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView>(x => x.Opacity, opacity));
            view.Animate(anim);
            view.Opacity = opacity;
            return view;
        }

        public static async Task<TView> FadeToAsync<TView>(this TView view, double opacity, uint length = 250, string easing = null) where TView : WView
        {
            var anim = new WAnimation();
            anim.Duration = length;
            anim.Fill = FillMode.Both;
            anim.Easing = easing;
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView>(x => x.Opacity, view.Opacity));
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView>(x => x.Opacity, opacity));
            view.Opacity = opacity;
            await view.AnimateAsync(anim);
            return view;
        }

        public static TView RotateTo<TView>(this TView view, double rotation, uint length = 250, string easing = null) where TView : WView
        {
            var anim = new WAnimation();
            anim.Duration = length;
            anim.Fill = FillMode.Both;
            anim.Easing = easing;
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView>(x => x.Rotation, view.Rotation));
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView>(x => x.Rotation, rotation));
            view.Animate(anim);
            view.Rotation = rotation;
            return view;
        }

        public static async Task<TView> RotateToAsync<TView>(this TView view, double rotation, uint length = 250, string easing = null) where TView : WView
        {
            var anim = new WAnimation();
            anim.Duration = length;
            anim.Fill = FillMode.Both;
            anim.Easing = easing;
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView>(x => x.Rotation, view.Rotation));
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView>(x => x.Rotation, rotation));
            view.Rotation = rotation;
            await view.AnimateAsync(anim);
            return view;
        }

        //public static TView RotateXTo<TView>(this TView view, double rotation, uint length = 250, string easing = null) where TView : WView
        //{
        //    var anim = new WAnimation();
        //    anim.Duration = length;
        //    anim.Fill = FillMode.Both;
        //    anim.Easing = easing;
        //    anim.Keyframes.Add(new WKeyframe().AddProperty(x => x.RotationX, view.RotationX));
        //    anim.Keyframes.Add(new WKeyframe().AddProperty(x => x.RotationX, rotation));
        //    view.Animate(anim);
        //    view.RotationX = rotation;
        //    return view;
        //}

        //public static async Task<TView> RotateXToAsync<TView>(this TView view, double rotation, uint length = 250, string easing = null) where TView : WView
        //{
        //    var anim = new WAnimation();
        //    anim.Duration = length;
        //    anim.Fill = FillMode.Both;
        //    anim.Easing = easing;
        //    anim.Keyframes.Add(new WKeyframe().AddProperty(x => x.RotationX, view.RotationX));
        //    anim.Keyframes.Add(new WKeyframe().AddProperty(x => x.RotationX, rotation));
        //    view.RotationX = rotation;
        //    await view.AnimateAsync(anim);
        //    return view;
        //}

        //public static TView RotateYTo<TView>(this TView view, double rotation, uint length = 250, string easing = null) where TView : WView
        //{
        //    var anim = new WAnimation();
        //    anim.Duration = length;
        //    anim.Fill = FillMode.Both;
        //    anim.Easing = easing;
        //    anim.Keyframes.Add(new WKeyframe().AddProperty(x => x.RotationY, view.RotationY));
        //    anim.Keyframes.Add(new WKeyframe().AddProperty(x => x.RotationY, rotation));
        //    view.Animate(anim);
        //    view.RotationY = rotation;
        //    return view;
        //}

        //public static async Task<TView> RotateYToAsync<TView>(this TView view, double rotation, uint length = 250, string easing = null) where TView : WView
        //{
        //    var anim = new WAnimation();
        //    anim.Duration = length;
        //    anim.Fill = FillMode.Both;
        //    anim.Easing = easing;
        //    anim.Keyframes.Add(new WKeyframe().AddProperty(x => x.RotationY, view.RotationY));
        //    anim.Keyframes.Add(new WKeyframe().AddProperty(x => x.RotationY, rotation));
        //    view.RotationY = rotation;
        //    await view.AnimateAsync(anim);
        //    return view;
        //}

        public static TView ScaleTo<TView>(this TView view, double scale, uint length = 250, string easing = null) where TView : WView
        {
            var anim = new WAnimation();
            anim.Duration = length;
            anim.Fill = FillMode.Both;
            anim.Easing = easing;
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView, Point3>(x => x.Scale, view.Scale));
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView, Point3>(x => x.Scale, scale));
            view.Animate(anim);
            view.Scale = scale;
            return view;
        }

        public static async Task<TView> ScaleToAsync<TView>(this TView view, double scale, uint length = 250, string easing = null) where TView : WView
        {
            var anim = new WAnimation();
            anim.Duration = length;
            anim.Fill = FillMode.Both;
            anim.Easing = easing;
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView, Point3>(x => x.Scale, view.Scale));
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView, Point3>(x => x.Scale, scale));
            view.Scale = scale;
            await view.AnimateAsync(anim);
            return view;
        }

        //public static TView ScaleXTo<TView>(this TView view, double scale, uint length = 250, string easing = null) where TView : WView
        //{
        //    var anim = new WAnimation();
        //    anim.Duration = length;
        //    anim.Fill = FillMode.Both;
        //    anim.Easing = easing;
        //    anim.Keyframes.Add(new WKeyframe().AddProperty(x => x.ScaleX, view.ScaleX));
        //    anim.Keyframes.Add(new WKeyframe().AddProperty(x => x.ScaleX, scale));
        //    view.Animate(anim);
        //    view.ScaleX = scale;
        //    return view;
        //}

        //public static async Task<TView> ScaleXToAsync<TView>(this TView view, double scale, uint length = 250, string easing = null) where TView : WView
        //{
        //    var anim = new WAnimation();
        //    anim.Duration = length;
        //    anim.Fill = FillMode.Both;
        //    anim.Easing = easing;
        //    anim.Keyframes.Add(new WKeyframe().AddProperty(x => x.ScaleX, view.ScaleX));
        //    anim.Keyframes.Add(new WKeyframe().AddProperty(x => x.ScaleX, scale));
        //    view.ScaleX = scale;
        //    await view.AnimateAsync(anim);
        //    return view;
        //}

        //public static TView ScaleYTo<TView>(this TView view, double scale, uint length = 250, string easing = null) where TView : WView
        //{
        //    var anim = new WAnimation();
        //    anim.Duration = length;
        //    anim.Fill = FillMode.Both;
        //    anim.Easing = easing;
        //    anim.Keyframes.Add(new WKeyframe().AddProperty(x => x.ScaleY, view.ScaleY));
        //    anim.Keyframes.Add(new WKeyframe().AddProperty(x => x.ScaleY, scale));
        //    view.Animate(anim);
        //    view.ScaleY = scale;
        //    return view;
        //}

        //public static async Task<TView> ScaleYToAsync<TView>(this TView view, double scale, uint length = 250, string easing = null) where TView : WView
        //{
        //    var anim = new WAnimation();
        //    anim.Duration = length;
        //    anim.Fill = FillMode.Both;
        //    anim.Easing = easing;
        //    anim.Keyframes.Add(new WKeyframe().AddProperty(x => x.ScaleY, view.ScaleY));
        //    anim.Keyframes.Add(new WKeyframe().AddProperty(x => x.ScaleY, scale));
        //    view.ScaleY = scale;
        //    await view.AnimateAsync(anim);
        //    return view;
        //}

        public static TView TranslateTo<TView>(this TView view, double x, double y, uint length = 250, string easing = null) where TView : WView
        {
            var anim = new WAnimation();
            anim.Duration = length;
            anim.Fill = FillMode.Both;
            anim.Easing = easing;
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView, WPoint>(x => x.Translation, view.Translation));
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView, WPoint>(x => x.Translation, new WPoint()
            {
                X = x,
                Y = y
            }));
            view.Animate(anim);
            view.TranslationX = x;
            view.TranslationY = y;
            return view;
        }

        public static async Task<TView> TranslateToAsync<TView>(this TView view, double x, double y, uint length = 250, string easing = null) where TView : WView
        {
            var anim = new WAnimation();
            anim.Duration = length;
            anim.Fill = FillMode.Both;
            anim.Easing = easing;
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView, WPoint>(x => x.Translation, view.Translation));
            anim.Keyframes.Add(new WKeyframe().AddProperty<WView, WPoint>(x => x.Translation, new WPoint()
            {
                X = x,
                Y = y
            }));
            view.TranslationX = x;
            view.TranslationY = y;
            await view.AnimateAsync(anim);
            return view;
        }
    }
}
