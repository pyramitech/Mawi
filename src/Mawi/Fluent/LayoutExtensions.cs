using Mawi.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Fluent
{
    public static class LayoutExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Orientation<TView>(this TView view, WStackOrientation orientation) where TView : WStackLayout
        {
            view.Orientation = orientation;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Spacing<TView>(this TView view, WLength value) where TView : WStackLayout
        {
            view.Spacing = value;
            return view;
        }


    }

    public static class ContainerExtensions
    {
        /// <summary>adds the children in order, null entries are skipped</summary>
        public static TView Children<TView>(this TView view, params WView[] children) where TView : WContainer
        {
            foreach (var child in children)
            {
                if (child != null)
                    view.Children.Add(child);
            }
            return view;
        }

        /// <summary>adds the children in order, null entries are skipped</summary>
        public static TView FillChildren<TView>(this TView view, IEnumerable<WView> children) where TView : WContainer
        {
            foreach (var child in children)
            {
                if (child != null)
                    view.Children.Add(child);
            }
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Remove<TView>(this TView view, WView child) where TView : WContainer
        {
            view.Children.Remove(child);
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Clear<TView>(this TView view) where TView : WContainer
        {
            view.Children.Clear();
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Content<TView>(this TView view, WView content) where TView : WContent
        {
            view.Content = content;
            return view;
        }
    }

    public static class GridExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Rows<TView>(this TView view, RowDefinitionCollection rows) where TView : WGrid
        {
            view.RowDefinitions = rows;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Columns<TView>(this TView view, ColumnDefinitionCollection columns) where TView : WGrid
        {
            view.ColumnDefinitions = columns;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView RowSpacing<TView>(this TView view, WLength spacing) where TView : WGrid
        {
            view.RowSpacing = spacing;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView ColumnSpacing<TView>(this TView view, WLength spacing) where TView : WGrid
        {
            view.ColumnSpacing = spacing;
            return view;
        }

        /// <summary>adds a child into the given cell, same argument order as MAUI's Grid.Add</summary>
        public static TView Add<TView>(this TView view, WView child, int column, int row) where TView : WGrid
        {
            child.LayoutSettings = new WGridLayoutSettings
            {
                Row = row,
                Column = column,
            };
            view.Children.Add(child);
            return view;
        }

        public static TView Row<TView>(this TView view, int row) where TView : WView
        {
            GridSettings(view).Row = row;
            return view;
        }

        public static TView Row<TView>(this TView view, int row, int span) where TView : WView
        {
            var settings = GridSettings(view);
            settings.Row = row;
            settings.RowSpan = span;
            return view;
        }

        public static TView Column<TView>(this TView view, int column) where TView : WView
        {
            GridSettings(view).Column = column;
            return view;
        }

        public static TView Column<TView>(this TView view, int column, int span) where TView : WView
        {
            var settings = GridSettings(view);
            settings.Column = column;
            settings.ColumnSpan = span;
            return view;
        }

        public static TView RowSpan<TView>(this TView view, int span) where TView : WView
        {
            GridSettings(view).RowSpan = span;
            return view;
        }

        public static TView ColumnSpan<TView>(this TView view, int span) where TView : WView
        {
            GridSettings(view).ColumnSpan = span;
            return view;
        }

        private static WGridLayoutSettings GridSettings(WView view)
        {
            if (view.LayoutSettings is not WGridLayoutSettings settings)
            {
                settings = new WGridLayoutSettings();
                view.LayoutSettings = settings;
            }
            return settings;
        }
    }

    public static class ScrollViewExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView Orientation<TView>(this TView view, ScrollOrientation orientation) where TView : WScrollView
        {
            view.Orientation = orientation;
            return view;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TView ScrollBarVisibility<TView>(this TView view, ScrollBarVisibility visibility) where TView : WScrollView
        {
            view.ScrollBarVisibility = visibility;
            return view;
        }
    }
}
