using Mawi.Views;
using System.Runtime.CompilerServices;

namespace Mawi.Fluent
{
    public static class ButtonExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Text<TView>(this TView view, string text) where TView : WButton
        {
            view.Text = text;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Source<TView>(this TView view, ImageSource source) where TView : WButton
        {
            view.Source = source;
            return view;
        }
    }

    public static class ImageExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Source<TView>(this TView view, ImageSource source) where TView : WImage
        {
            view.Source = source;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Aspect<TView>(this TView view, Aspect aspect) where TView : WImage
        {
            view.Aspect = aspect;
            return view;
        }
    }

    public static class ActivityIndicatorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView IsRunning<TView>(this TView view, bool running) where TView : WActivityIndicator
        {
            view.IsRunning = running;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Color<TView>(this TView view, Color color) where TView : WActivityIndicator
        {
            view.Color = color;
            return view;
        }
    }
}
