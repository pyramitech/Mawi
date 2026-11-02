using Mawi.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Fluent
{
    public static class BaseExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Width<TView>(this TView view, WLength value) where TView : WView
        {
            view.WidthRequest = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView MinWidth<TView>(this TView view, WLength value) where TView : WView
        {
            view.MinimumWidth = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView MaxWidth<TView>(this TView view, WLength value) where TView : WView
        {
            view.MaximumWidth = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Height<TView>(this TView view, WLength value) where TView : WView
        {
            view.HeightRequest = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView MinHeight<TView>(this TView view, WLength value) where TView : WView
        {
            view.MinimumHeight = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView MaxHeight<TView>(this TView view, WLength value) where TView : WView
        {
            view.MaximumHeight = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Size<TView>(this TView view, WLength size) where TView : WView
        {
            view.WidthRequest = size;
            view.HeightRequest = size;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Size<TView>(this TView view, WLength width, WLength height) where TView : WView
        {
            view.WidthRequest = width;
            view.HeightRequest = height;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Margin<TView>(this TView view, WThickness value) where TView : WView
        {
            view.Margin = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Margin<TView>(this TView view, WLength horizontal, WLength vertical) where TView : WView
        {
            view.Margin = new WThickness(horizontal, vertical);
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Margin<TView>(this TView view, WLength value) where TView : WView
        {
            view.Margin = new WThickness(value);
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Padding<TView>(this TView view, WThickness value) where TView : WView
        {
            view.Padding = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Padding<TView>(this TView view, WLength horizontal, WLength vertical) where TView : WView
        {
            view.Padding = new WThickness(horizontal, vertical);
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Padding<TView>(this TView view, WLength value) where TView : WView
        {
            view.Padding = new WThickness(value);
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Start<TView>(this TView view) where TView : WView
        {
            view.HorizontalOptions = LayoutOptions.Start;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView CenterHorizontal<TView>(this TView view) where TView : WView
        {
            view.HorizontalOptions = LayoutOptions.Center;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView FillHorizontal<TView>(this TView view) where TView : WView
        {
            view.HorizontalOptions = LayoutOptions.Fill;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView End<TView>(this TView view) where TView : WView
        {
            view.HorizontalOptions = LayoutOptions.End;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Top<TView>(this TView view) where TView : WView
        {
            view.VerticalOptions = LayoutOptions.Start;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView CenterVertical<TView>(this TView view) where TView : WView
        {
            view.VerticalOptions = LayoutOptions.Center;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Bottom<TView>(this TView view) where TView : WView
        {
            view.VerticalOptions = LayoutOptions.End;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView FillVertical<TView>(this TView view) where TView : WView
        {
            view.VerticalOptions = LayoutOptions.Fill;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Center<TView>(this TView view) where TView : WView
        {
            view.HorizontalOptions = LayoutOptions.Center;
            view.VerticalOptions = LayoutOptions.Center;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Fill<TView>(this TView view) where TView : WView
        {
            view.HorizontalOptions = LayoutOptions.Fill;
            view.VerticalOptions = LayoutOptions.Fill;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Background<TView>(this TView view, Brush brush) where TView : WView
        {
            view.Background = brush;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView BackgroundColor<TView>(this TView view, Color color) where TView : WView
        {
            view.BackgroundColor = color;      
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView InputTransparent<TView>(this TView view, bool value) where TView : WView
        {
            view.InputTransparent = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView IsEnabled<TView>(this TView view, bool value) where TView : WView
        {
            view.IsEnabled = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView IsVisible<TView>(this TView view, bool value) where TView : WView
        {
            view.IsVisible = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Opacity<TView>(this TView view, double value) where TView : WView
        {
            view.Opacity = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Rotation<TView>(this TView view, double value) where TView : WView
        {
            view.Rotation = value;
            return view;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static TView RotationX<TView>(this TView view, double value) where TView : WView
        //{
        //    view.RotationX = value;
        //    return view;
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static TView RotationY<TView>(this TView view, double value) where TView : WView
        //{
        //    view.RotationY = value;
        //    return view;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Scale<TView>(this TView view, double value) where TView : WView
        {
            view.Scale = value;
            return view;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static TView ScaleX<TView>(this TView view, double value) where TView : WView
        //{
        //    view.ScaleX = value;
        //    return view;
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static TView ScaleY<TView>(this TView view, double value) where TView : WView
        //{
        //    view.ScaleY = value;
        //    return view;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView TranslationX<TView>(this TView view, WLength value) where TView : WView
        {
            view.TranslationX = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView TranslationY<TView>(this TView view, WLength value) where TView : WView
        {
            view.TranslationY = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView ZIndex<TView>(this TView view, int value) where TView : WView
        {
            view.ZIndex = value;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Id<TView>(this TView view, ref TView value) where TView : WView
        {
            value = view;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Apply<TView>(this TView view, Action<TView> action) where TView : WView
        {
            action(view);
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView BorderRadius<TView>(this TView view, WThickness radius) where TView : WView
        {
            view.BorderRadius = radius;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView BorderRadius<TView>(this TView view, WLength horizontal, WLength vertical) where TView : WView
        {
            view.BorderRadius = new WThickness(horizontal, vertical);
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView BorderRadius<TView>(this TView view, WLength left, WLength top, WLength right, WLength bottom) where TView : WView
        {
            view.BorderRadius = new WThickness(left, top, right, bottom);
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView BorderColor<TView>(this TView view, Color color) where TView : WView
        {
            view.BorderColor = color;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView BorderWidth<TView>(this TView view, WThickness radius) where TView : WView
        {
            view.BorderWidth = radius;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView BorderStyle<TView>(this TView view, CssBorderStyle color) where TView : WView
        {
            view.BorderStyle = color;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Margin<TView>(this TView view, WLength left, WLength top, WLength right, WLength bottom) where TView : WView
        {
            view.Margin = new WThickness(left, top, right, bottom);
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Shadow<TView>(this TView view, params WShadow[] shadows) where TView : WView
        {
            view.Shadow = new WShadowList(shadows);
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Shadow<TView>(this TView view, WShadowList shadows) where TView : WView
        {
            view.Shadow = shadows;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Filter<TView>(this TView view, params WFilter[] filters) where TView : WView
        {
            view.Filter = new WFilterList(filters);
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Filter<TView>(this TView view, WFilterList filters) where TView : WView
        {
            view.Filter = filters;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Backdrop<TView>(this TView view, params WFilter[] filters) where TView : WView
        {
            view.Backdrop = new WFilterList(filters);
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Backdrop<TView>(this TView view, WFilterList filters) where TView : WView
        {
            view.Backdrop = filters;
            return view;
        }

        /// <summary>subscribes to Click</summary>
        public static TView Tap<TView>(this TView view, Action action) where TView : WElement
        {
            view.Click += (s, e) => action();
            return view;
        }

        /// <summary>subscribes to Click</summary>
        public static TView Tap<TView>(this TView view, Action<object, EventArgs> action) where TView : WElement
        {
            view.Click += (s, e) => action(s, e);
            return view;
        }

    }
}
