using Mawi.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;

namespace Mawi.Views
{
    public enum WCrossAxisOverflow
    {
        Hidden = 0,
        Scroll
    }

    /// <summary>
    /// virtualized scrolling list with estimated item sizes: only the visible window
    /// (plus overscan) exists in the DOM. items are measured as they are rendered and
    /// the running average is used as the assumed size of every not-yet-rendered item,
    /// so templates are free to produce any size on the scroll axis. realized item
    /// views get their data object as BindingContext, so {w:Bind} works in templates.
    ///
    /// structure is a fixed-size scroll element containing one inner panel that carries
    /// the spacer paddings (padding on the scroller itself would inflate its own
    /// border-box). the window is anchored: measurement corrections are absorbed into
    /// the leading padding so already-visible content never moves on screen, the scroll
    /// position is only corrected when the window reaches the very start of the list
    /// </summary>
    public class WVirtualList : WView
    {
        private IList _itemsSource;
        private Func<object, WView> _itemTemplate;
        private WStackOrientation _orientation = WStackOrientation.Vertical;

        private readonly WVirtualPanel _panel;

        private double[] _sizes; //measured main-axis size per index, NaN = not measured yet
        private double _measuredSum;
        private int _measuredCount;

        private readonly List<WView> _window = new(64); //realized views for [_first.._last]
        private readonly List<WView> _pendingAppend = new(32);
        private readonly List<WView> _pendingPrepend = new(32);
        private int _first = -1;
        private int _last = -1;

        private WCrossAxisOverflow _crossAxisOverflow = WCrossAxisOverflow.Hidden;

        private double _viewportMain;
        private double _scrollMain;
        private double _topPad;
        private double _bottomPad;
        private int _version; //window generation, stale measure messages are discarded

        public WVirtualList()
        {
            _panel = new WVirtualPanel(this);

            this.Scrolled += (s, e) =>
            {
                _scrollMain = IsVertical ? _scrollY : _scrollX;
                UpdateWindow();
            };
            this.SizeChanged += (s, e) =>
            {
                _viewportMain = IsVertical ? Height : Width;
                UpdateWindow();
            };
        }

        /// <summary>items to display. INotifyCollectionChanged sources trigger a full reset on change</summary>
        public IList ItemsSource
        {
            get => _itemsSource;
            set
            {
                if (_itemsSource is INotifyCollectionChanged oldIncc)
                {
                    oldIncc.CollectionChanged -= OnItemsChanged;
                }
                _itemsSource = value;
                if (value is INotifyCollectionChanged incc)
                {
                    incc.CollectionChanged += OnItemsChanged;
                }
                ResetItems();
            }
        }

        /// <summary>creates the view for one item. the item is also set as the view's BindingContext</summary>
        public Func<object, WView> ItemTemplate
        {
            get => _itemTemplate;
            set
            {
                _itemTemplate = value;
                ResetItems();
            }
        }

        /// <summary>scroll axis, only Vertical and Horizontal are supported</summary>
        public WStackOrientation Orientation
        {
            get => _orientation;
            set
            {
                _orientation = value;
                Invalidate();
                ResetItems();
            }
        }

        /// <summary>
        /// whether the non-virtualized axis clips (default) or scrolls. with Scroll, items
        /// wider than the viewport (a nowrap text line, for instance) can be panned to instead
        /// of being cut off. virtualization still only happens on the main axis
        /// </summary>
        public WCrossAxisOverflow CrossAxisOverflow
        {
            get => _crossAxisOverflow;
            set
            {
                _crossAxisOverflow = value;
                MarkDirty(UpdateState.DirtyStyle);
                _panel.MarkDirty(UpdateState.DirtyStyle);
            }
        }

        /// <summary>items rendered beyond the visible range on each side</summary>
        public int OverscanCount { get; set; } = 4;

        /// <summary>assumed item size on the scroll axis until the first real measurement arrives</summary>
        public double EstimatedItemSize { get; set; } = 44;

        internal bool IsVertical => _orientation != WStackOrientation.Horizontal && _orientation != WStackOrientation.HorizontalReverse;

        internal List<WView> Window => _window;
        internal double TopPad => _topPad;
        internal double BottomPad => _bottomPad;

        private double Estimate => _measuredCount > 0
            ? Math.Max(1, _measuredSum / _measuredCount)
            : Math.Max(1, EstimatedItemSize);

        private double SizeOf(int index)
            => _sizes != null && !double.IsNaN(_sizes[index]) ? _sizes[index] : Estimate;

        private double SumSizes(int from, int toExclusive)
        {
            double sum = 0;
            for (int i = from; i < toExclusive; i++)
            {
                sum += SizeOf(i);
            }
            return sum;
        }

        /// <summary>scrolls to the offset of the given index, using estimates for unmeasured items</summary>
        public void ScrollToIndex(int index)
        {
            if (_sizes == null || _sizes.Length == 0)
                return;

            index = Math.Clamp(index, 0, _sizes.Length - 1);

            //offsets are anchored to the current window, not to a global prefix sum
            double offset;
            if (_first == -1)
            {
                offset = SumSizes(0, index);
            }
            else if (index >= _first)
            {
                offset = _topPad + SumSizes(_first, index);
            }
            else
            {
                offset = _topPad - SumSizes(index, _first);
            }

            ScrollToOffset(Math.Max(0, offset));
        }

        public void ScrollToOffset(double offset)
        {
            _scrollMain = Math.Max(0, offset);
            //the scroll command goes first so the measure issued by UpdateWindow reads the
            //new position. the paddings already span the full estimated size, so the target
            //offset exists before the destination window is even rendered
            ExecuteScroll();
            UpdateWindow();
        }

        private void ExecuteScroll()
        {
            Root?.Execute(IsVertical
                ? $"scrollTo(\"w{Id}\", 0, {_scrollMain.ToInv()}, \"instant\");"
                : $"scrollTo(\"w{Id}\", {_scrollMain.ToInv()}, 0, \"instant\");");
        }

        private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e) => ResetItems();

        private void ResetItems()
        {
            ++_version;

            foreach (var view in _window)
            {
                RemoveItemView(view);
            }
            _window.Clear();
            _pendingAppend.Clear();
            _pendingPrepend.Clear();
            _first = _last = -1;

            int count = _itemsSource?.Count ?? 0;
            _sizes = new double[count];
            Array.Fill(_sizes, double.NaN);
            _measuredSum = 0;
            _measuredCount = 0;
            _topPad = _bottomPad = 0;

            _panel.MarkDirty(UpdateState.DirtyStyle);
            UpdateWindow();
        }

        private void UpdateWindow()
        {
            if (Root == null || _itemTemplate == null)
                return;

            int count = _itemsSource?.Count ?? 0;
            if (count == 0)
            {
                ApplyWindow(-1, -1);
                return;
            }
            if (_viewportMain <= 0)
                return; //not laid out yet, the resize observer will call us

            double scroll = Math.Max(0, _scrollMain);

            //locate the first visible item, walking from the current window anchor
            //(or from the top when there is no window yet)
            int firstVisible;
            double firstOffset;
            if (_first == -1)
            {
                double off = 0;
                int i = 0;
                for (; i < count - 1; i++)
                {
                    double size = SizeOf(i);
                    if (off + size > scroll)
                        break;
                    off += size;
                }
                firstVisible = i;
                firstOffset = off;
            }
            else
            {
                double off = _topPad;
                int i = _first;
                while (i > 0 && off > scroll)
                {
                    --i;
                    off -= SizeOf(i);
                }
                while (i < count - 1 && off + SizeOf(i) <= scroll)
                {
                    off += SizeOf(i);
                    ++i;
                }
                firstVisible = i;
                firstOffset = off;
            }

            int lastVisible = firstVisible;
            double end = scroll + _viewportMain;
            double offEnd = firstOffset;
            for (int i = firstVisible; i < count; i++)
            {
                lastVisible = i;
                offEnd += SizeOf(i);
                if (offEnd >= end)
                    break;
            }

            ApplyWindow(
                Math.Max(0, firstVisible - OverscanCount),
                Math.Min(count - 1, lastVisible + OverscanCount));
        }

        private void ApplyWindow(int newFirst, int newLast)
        {
            bool changed = false;

            if (newFirst == -1 || newLast < newFirst)
            {
                if (_window.Count != 0)
                {
                    foreach (var view in _window)
                    {
                        RemoveItemView(view);
                    }
                    _window.Clear();
                    changed = true;
                }
                _first = _last = -1;
                _topPad = 0;
            }
            else if (_first == -1 || newLast < _first || newFirst > _last)
            {
                //disjoint from the current window, full rebuild anchored at the estimated offset
                foreach (var view in _window)
                {
                    RemoveItemView(view);
                }
                _window.Clear();
                for (int i = newFirst; i <= newLast; i++)
                {
                    var view = CreateItemView(i);
                    _window.Add(view);
                    _pendingAppend.Add(view);
                }
                _first = newFirst;
                _last = newLast;
                _topPad = SumSizes(0, newFirst);
                changed = true;
            }
            else
            {
                while (_first < newFirst)
                {
                    RemoveItemView(_window[0]);
                    _window.RemoveAt(0);
                    _topPad += SizeOf(_first); //the removed item's space becomes padding
                    ++_first;
                    changed = true;
                }
                while (_last > newLast)
                {
                    RemoveItemView(_window[^1]);
                    _window.RemoveAt(_window.Count - 1);
                    --_last;
                    changed = true;
                }
                if (newFirst < _first)
                {
                    var prepends = new List<WView>(_first - newFirst);
                    for (int i = newFirst; i < _first; i++)
                    {
                        prepends.Add(CreateItemView(i));
                        _topPad -= SizeOf(i); //the new item takes its space back from the padding
                    }
                    _window.InsertRange(0, prepends);
                    _pendingPrepend.AddRange(prepends);
                    _first = newFirst;
                    changed = true;
                }
                if (newLast > _last)
                {
                    for (int i = _last + 1; i <= newLast; i++)
                    {
                        var view = CreateItemView(i);
                        _window.Add(view);
                        _pendingAppend.Add(view);
                    }
                    _last = newLast;
                    changed = true;
                }
            }

            double oldBottom = _bottomPad;
            _bottomPad = _last == -1 ? 0 : SumSizes(_last + 1, _itemsSource?.Count ?? 0);

            ClampTopBoundary();

            if (changed)
            {
                if (_pendingAppend.Count != 0 || _pendingPrepend.Count != 0)
                {
                    _panel.MarkDirty(UpdateState.DirtyAdd);
                    ++_version;
                    Root.Execute($"measureChildren(\"w{_panel.Id}\",{_version});");
                }
                _panel.MarkDirty(UpdateState.DirtyStyle);
            }
            else if (oldBottom != _bottomPad)
            {
                _panel.MarkDirty(UpdateState.DirtyStyle);
            }
        }

        /// <summary>
        /// the anchored padding may drift from the true value, which only ever becomes
        /// observable at the very start of the list: item 0 must sit at offset 0
        /// </summary>
        private void ClampTopBoundary()
        {
            double shift = 0;
            if (_first == 0 && _topPad != 0)
            {
                shift = -_topPad;
            }
            else if (_topPad < 0)
            {
                shift = -_topPad;
            }

            if (shift != 0)
            {
                _topPad += shift;
                _scrollMain = Math.Max(0, _scrollMain + shift);
                _panel.MarkDirty(UpdateState.DirtyStyle);
                ExecuteScroll();
            }
        }

        private WView CreateItemView(int index)
        {
            var item = _itemsSource[index];
            var view = _itemTemplate(item);
            view.Parent = _panel;
            view.BindingContext = item;
            if (Root != null)
            {
                view.AssignRoot(Root);
            }
            return view;
        }

        private void RemoveItemView(WView view)
        {
            if (view.Root != null)
            {
                view.Root.RemoveView(view);
            }
            view.UpdateState = UpdateState.Removed;
        }

        /// <summary>
        /// result of measureChildren on the panel: version, scroller scrollTop/scrollLeft/
        /// clientWidth/clientHeight, child count, then width/height per child.
        /// may arrive on the js bridge thread
        /// </summary>
        internal void InvokeChildrenMeasured(ReadOnlySpan<char> args, MemoryExtensions.SpanSplitEnumerator<char> ranges)
        {
            static double NextDouble(ReadOnlySpan<char> a, ref MemoryExtensions.SpanSplitEnumerator<char> r)
            {
                var value = double.Parse(a.Slice(r.Current.Start.Value, r.Current.End.Value - r.Current.Start.Value), CultureInfo.InvariantCulture);
                r.MoveNext();
                return value;
            }

            int version = (int)NextDouble(args, ref ranges);
            double scrollTop = NextDouble(args, ref ranges);
            double scrollLeft = NextDouble(args, ref ranges);
            double clientWidth = NextDouble(args, ref ranges);
            double clientHeight = NextDouble(args, ref ranges);
            int count = (int)NextDouble(args, ref ranges);

            var main = new double[count];
            for (int i = 0; i < count; i++)
            {
                double w = NextDouble(args, ref ranges);
                double h = NextDouble(args, ref ranges);
                main[i] = IsVertical ? h : w;
            }

            MainThread.BeginInvokeOnMainThread(() => ApplyMeasurements(version, scrollTop, scrollLeft, clientWidth, clientHeight, main));
        }

        private void ApplyMeasurements(int version, double scrollTop, double scrollLeft, double clientWidth, double clientHeight, double[] main)
        {
            if (version != _version || _sizes == null || _first == -1 || main.Length != _window.Count)
                return; //stale, a newer window is already on its way

            _viewportMain = IsVertical ? clientHeight : clientWidth;
            //deliberately NOT taking scrollTop from the reply: it can be stale relative to a
            //pending programmatic scroll, the scroll events are the only source of truth

            //replace assumed sizes with measured ones. size changes fully above the
            //viewport are absorbed into the leading padding so nothing moves on screen
            double offset = _topPad;
            double aboveDelta = 0;
            for (int i = 0; i < main.Length; i++)
            {
                int index = _first + i;
                double old = SizeOf(index);
                double measured = Math.Max(0, main[i]);

                if (!double.IsNaN(_sizes[index]))
                {
                    _measuredSum -= _sizes[index];
                    --_measuredCount;
                }
                _sizes[index] = measured;
                _measuredSum += measured;
                ++_measuredCount;

                if (offset + old <= _scrollMain + 0.5)
                {
                    aboveDelta += measured - old;
                }
                offset += old;
            }

            _topPad -= aboveDelta;
            _bottomPad = SumSizes(_last + 1, _itemsSource?.Count ?? 0);

            _panel.MarkDirty(UpdateState.DirtyStyle);
            ClampTopBoundary();

            //the estimates just improved, the window might need to grow (initial fill) or shift
            UpdateWindow();
        }

        #region rendering

        public override void Render(StringBuilder sb)
        {
            this.UpdateState = UpdateState.None;

            sb.Append("<w-scroll");
            RenderInternals(sb);
            RenderEvents(sb);
            sb.Append(" style=\\\"");
            RenderStyle(sb);
            sb.Append("\\\">");

            _panel.Render(sb);

            sb.Append("</w-scroll>");
        }

        public override void RenderStyle(StringBuilder sb)
        {
            string cross = _crossAxisOverflow == WCrossAxisOverflow.Scroll ? "auto;" : "hidden;";
            sb.Append(IsVertical ? "overflow-y:auto;overflow-x:" : "overflow-x:auto;overflow-y:").Append(cross);
            sb.Append("display:block;overflow-anchor:none;");

            RenderBackground(sb);
            RenderPosition(sb);
            RenderTransform(sb);
            RenderBorder(sb);

            Parent.RenderChildLayout(this, sb);
        }

        //lays out the inner panel inside the scroller
        public override void RenderChildLayout(WView child, StringBuilder sb)
        {
            //with cross-axis scrolling the panel grows to its content on the cross axis
            //(min 100% so short content still fills the viewport), letting the scroller pan it
            bool cross = _crossAxisOverflow == WCrossAxisOverflow.Scroll;
            if (!IsVertical)
            {
                sb.Append("width:max-content;");
                sb.Append(cross ? "min-height:100%;height:max-content;" : "height:100%;");
            }
            else if (cross)
            {
                sb.Append("min-width:100%;width:max-content;");
            }
        }

        internal void RenderPendingAppends(StringBuilder sb)
        {
            foreach (var view in _pendingAppend)
            {
                view.Render(sb);
            }
            _pendingAppend.Clear();
        }

        internal bool HasPendingPrepends => _pendingPrepend.Count != 0;

        internal void RenderPendingPrepends(StringBuilder sb)
        {
            foreach (var view in _pendingPrepend)
            {
                view.Render(sb);
            }
            _pendingPrepend.Clear();
        }

        internal void ClearPending()
        {
            _pendingAppend.Clear();
            _pendingPrepend.Clear();
        }

        #endregion

        internal override void AssignRoot(WRoot root)
        {
            base.AssignRoot(root);
            _panel.AssignRoot(root);
            foreach (var view in _window)
            {
                view.AssignRoot(root);
            }
            UpdateWindow();

            //if reattaching move back scroll to where it was
            if (_scrollMain > 0)
            {
                ExecuteScroll();
            }
        }

        internal override void UnassignRoot()
        {
            base.UnassignRoot();
            _panel.UnassignRoot();
            foreach (var view in _window)
            {
                view.UnassignRoot();
            }
        }
    }

    /// <summary>
    /// the inner element of a WVirtualList: hosts the realized item views and carries
    /// the spacer paddings that stand in for all unrendered items
    /// </summary>
    internal sealed class WVirtualPanel : WView, ICustomContainer, IPrependContainer
    {
        private readonly WVirtualList _list;

        public WVirtualPanel(WVirtualList list)
        {
            _list = list;
            Parent = list;
        }

        internal void OnChildrenMeasured(ReadOnlySpan<char> args, MemoryExtensions.SpanSplitEnumerator<char> ranges)
            => _list.InvokeChildrenMeasured(args, ranges);

        public override void Render(StringBuilder sb)
        {
            this.UpdateState = UpdateState.None;
            _list.ClearPending(); //a full render below covers anything pending

            sb.Append("<w-vlist");
            RenderInternals(sb);
            sb.Append(" style=\\\"");
            RenderStyle(sb);
            sb.Append("\\\">");

            foreach (var view in _list.Window)
            {
                view.Render(sb);
            }

            sb.Append("</w-vlist>");
        }

        public override void RenderStyle(StringBuilder sb)
        {
            bool vertical = _list.IsVertical;

            sb.Append("display:flex;flex-direction:").Append(vertical ? "column;" : "row;");

            sb.Append(vertical ? "padding-top:" : "padding-left:").Append(Math.Max(0, _list.TopPad).ToInv()).Append("px;");
            sb.Append(vertical ? "padding-bottom:" : "padding-right:").Append(Math.Max(0, _list.BottomPad).ToInv()).Append("px;");

            Parent.RenderChildLayout(this, sb);
        }

        public override void RenderChildLayout(WView child, StringBuilder sb)
        {
            bool vertical = _list.IsVertical;
            sb.Append("flex:0 0 auto;");

            sb.Append("align-self:");
            switch (vertical ? child.HorizontalOptions.Alignment : child.VerticalOptions.Alignment)
            {
                case LayoutAlignment.Fill:
                    if ((vertical ? child.WidthRequest : child.HeightRequest) == -1)
                    {
                        sb.Append("stretch");
                    }
                    else
                    {
                        sb.Append("center");
                    }
                    break;

                case LayoutAlignment.Start:
                    sb.Append("start");
                    break;

                case LayoutAlignment.Center:
                    sb.Append("center");
                    break;

                case LayoutAlignment.End:
                    sb.Append("end");
                    break;
            }
            sb.Append(';');
        }

        void ICustomContainer.RenderNewViews(StringBuilder sb) => _list.RenderPendingAppends(sb);

        bool IPrependContainer.HasPrependViews => _list.HasPendingPrepends;

        void IPrependContainer.RenderPrependViews(StringBuilder sb) => _list.RenderPendingPrepends(sb);
    }
}
