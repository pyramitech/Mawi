using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Views
{
    public abstract class WElement : Element
    {
        public WElement()
        {
            Id = WRootRuntime.RequestId();
        }

        internal new long Id;
        internal WRoot Root { get; set; }
        internal abstract void AssignRoot(WRoot root);
        internal abstract void UnassignRoot();

        private List<TaskCompletionSource> _nextTaskCompletion = new();

        internal UpdateState UpdateState = UpdateState.New;

        private List<WBindingExpression> _bindings;

        internal void AddBinding(WBindingExpression expression)
        {
            (_bindings ??= new(4)).Add(expression);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (_bindings != null)
            {
                foreach (var binding in _bindings)
                {
                    binding.Apply();
                }
            }
        }

        private readonly List<JsCommonEvent> _activeEvents = new List<JsCommonEvent>();
        public IReadOnlyList<JsCommonEvent> ActiveEvents => _activeEvents.AsReadOnly();

        protected EventHandler<PointerEventArgs> _click;
        protected EventHandler<PointerEventArgs> _dblclick;
        protected EventHandler<PointerEventArgs> _pointerdown;
        protected EventHandler<PointerEventArgs> _pointermove;
        protected EventHandler<PointerEventArgs> _pointerup;
        protected EventHandler<PointerEventArgs> _pointerenter;
        protected EventHandler<PointerEventArgs> _pointerleave;
        protected EventHandler<PointerEventArgs> _pointerover;
        protected EventHandler<PointerEventArgs> _pointerout;
        protected EventHandler<PointerEventArgs> _pointercancel;
        protected EventHandler<KeyEventArgs> _keydown;
        protected EventHandler<KeyEventArgs> _keyup;
        protected EventHandler _focus;
        protected EventHandler _blur;
        protected EventHandler _scroll;
        protected EventHandler<InputEventArgs> _input;
        protected EventHandler<InputEventArgs> _change;
        protected EventHandler<SelectionChangeEventArgs> _selectionchange;
        protected EventHandler<WheelEventArgs> _wheel;
        protected EventHandler _sizeChanged;
        protected bool _observeSize = false;

        protected void ClearUpdateState() => UpdateState = UpdateState.None;

        /// <summary>
        /// atomically sets the given dirty flags and queues the element for render when it
        /// transitions out of a clean state, safe to call from any thread.
        /// already-set flags (including a pending full Dirty or New) are left untouched
        /// </summary>
        internal void MarkDirty(UpdateState flags)
        {
            ref int loc = ref Unsafe.As<UpdateState, int>(ref UpdateState);
            int flagBits = (int)flags;

            int current = Volatile.Read(ref loc);

            bool wasNone;
            while (true)
            {
                if ((current & flagBits) == flagBits)
                    return; //someone else set it

                wasNone = current == 0;
                int prev = Interlocked.CompareExchange(ref loc, current | flagBits, current);
                if (prev == current) break;

                current = prev;
            }

            if (wasNone && Root != null && this is WView view)
            {
                Root.UpdateView(view);
            }
        }

        protected void RenderEvents(StringBuilder sb)
        {
            foreach (var ev in ActiveEvents)
            {
                var evName = ev.ToHtmlEventName();
                sb.Append(evName).Append("=\\\"_").Append(evName).Append("(event)\\\" ");
            }
        }

        protected void AddToActiveEvents(JsCommonEvent jsEvent, EventHandler handler)
        {
            //should be 1 if just added
            if (handler?.GetInvocationList().Length == 1 && !_activeEvents.Contains(jsEvent))
            {
                UpdateState |= UpdateState.DirtyEvent;
                _activeEvents.Add(jsEvent);
            }
        }

        protected void AddToActiveEvents<T>(JsCommonEvent jsEvent, EventHandler<T> handler)
        {
            //should be 1 if just added
            if (handler?.GetInvocationList().Length == 1 && !_activeEvents.Contains(jsEvent))
            {
                UpdateState |= UpdateState.DirtyEvent;
                _activeEvents.Add(jsEvent);
            }
        }

        protected void RemoveFromActiveEvents(JsCommonEvent jsEvent, EventHandler handler)
        {
            //should be 0 or null if just removed
            if (handler == null || handler.GetInvocationList().Length == 0)
            {
                UpdateState |= UpdateState.DirtyEvent;
                _activeEvents.Remove(jsEvent);
            }
        }

        protected void RemoveFromActiveEvents<T>(JsCommonEvent jsEvent, EventHandler<T> handler)
        {
            //should be 0 or null if just removed
            if (handler == null || handler.GetInvocationList().Length == 0)
            {
                UpdateState |= UpdateState.DirtyEvent;
                _activeEvents.Remove(jsEvent);
            }
        }

        public event EventHandler<PointerEventArgs> Click
        {
            add
            {
                _click += value;
                AddToActiveEvents(JsCommonEvent.Click, _click);
            }
            remove
            {
                _click -= value;
                RemoveFromActiveEvents(JsCommonEvent.Click, _click);
            }
        }

        public event EventHandler<PointerEventArgs> DoubleClick
        {
            add
            {
                _dblclick += value;
                AddToActiveEvents(JsCommonEvent.Dblclick, _dblclick);
            }
            remove
            {
                _dblclick -= value;
                RemoveFromActiveEvents(JsCommonEvent.Dblclick, _dblclick);
            }
        }

        public event EventHandler<PointerEventArgs> PointerDown
        {
            add
            {
                _pointerdown += value;
                AddToActiveEvents(JsCommonEvent.Pointerdown, _pointerdown);
            }
            remove
            {
                _pointerdown -= value;
                RemoveFromActiveEvents(JsCommonEvent.Pointerdown, _pointerdown);
            }
        }

        public event EventHandler<PointerEventArgs> PointerMove
        {
            add
            {
                _pointermove += value;
                AddToActiveEvents(JsCommonEvent.Pointermove, _pointermove);
            }
            remove
            {
                _pointermove -= value;
                RemoveFromActiveEvents(JsCommonEvent.Pointermove, _pointermove);
            }
        }

        public event EventHandler<PointerEventArgs> PointerUp
        {
            add
            {
                _pointerup += value;
                AddToActiveEvents(JsCommonEvent.Pointerup, _pointerup);
            }
            remove
            {
                _pointerup -= value;
                RemoveFromActiveEvents(JsCommonEvent.Pointerup, _pointerup);
            }
        }

        public event EventHandler<PointerEventArgs> PointerEnter
        {
            add
            {
                _pointerenter += value;
                AddToActiveEvents(JsCommonEvent.Pointerenter, _pointerenter);
            }
            remove
            {
                _pointerenter -= value;
                RemoveFromActiveEvents(JsCommonEvent.Pointerenter, _pointerenter);
            }
        }

        public event EventHandler<PointerEventArgs> PointerLeave
        {
            add
            {
                _pointerleave += value;
                AddToActiveEvents(JsCommonEvent.Pointerleave, _pointerleave);
            }
            remove
            {
                _pointerleave -= value;
                RemoveFromActiveEvents(JsCommonEvent.Pointerleave, _pointerleave);
            }
        }

        public event EventHandler<PointerEventArgs> PointerOver
        {
            add
            {
                _pointerover += value;
                AddToActiveEvents(JsCommonEvent.Pointerover, _pointerover);
            }
            remove
            {
                _pointerover -= value;
                RemoveFromActiveEvents(JsCommonEvent.Pointerover, _pointerover);
            }
        }

        public event EventHandler<PointerEventArgs> PointerOut
        {
            add
            {
                _pointerout += value;
                AddToActiveEvents(JsCommonEvent.Pointerout, _pointerout);
            }
            remove
            {
                _pointerout -= value;
                RemoveFromActiveEvents(JsCommonEvent.Pointerout, _pointerout);
            }
        }

        public event EventHandler<PointerEventArgs> PointerCancel
        {
            add
            {
                _pointercancel += value;
                AddToActiveEvents(JsCommonEvent.Pointercancel, _pointercancel);
            }
            remove
            {
                _pointercancel -= value;
                RemoveFromActiveEvents(JsCommonEvent.Pointercancel, _pointercancel);
            }
        }

        public event EventHandler<KeyEventArgs> KeyDown
        {
            add
            {
                _keydown += value;
                AddToActiveEvents(JsCommonEvent.Keydown, _keydown);
            }
            remove
            {
                _keydown -= value;
                RemoveFromActiveEvents(JsCommonEvent.Keydown, _keydown);
            }
        }

        public event EventHandler<KeyEventArgs> KeyUp
        {
            add
            {
                _keyup += value;
                AddToActiveEvents(JsCommonEvent.Keyup, _keyup);
            }
            remove
            {
                _keyup -= value;
                RemoveFromActiveEvents(JsCommonEvent.Keyup, _keyup);
            }
        }


        public event EventHandler Focused
        {
            add
            {
                _focus += value;
                AddToActiveEvents(JsCommonEvent.Focus, _focus);
            }
            remove
            {
                _focus -= value;
                RemoveFromActiveEvents(JsCommonEvent.Focus, _focus);
            }
        }

        public event EventHandler Unfocused
        {
            add
            {
                _blur += value;
                AddToActiveEvents(JsCommonEvent.Blur, _blur);
            }
            remove
            {
                _blur -= value;
                RemoveFromActiveEvents(JsCommonEvent.Blur, _blur);
            }
        }

        public event EventHandler Scrolled
        {
            add
            {
                _scroll += value;
                AddToActiveEvents(JsCommonEvent.Scroll, _scroll);
            }
            remove
            {
                _scroll -= value;
                RemoveFromActiveEvents(JsCommonEvent.Scroll, _scroll);
            }
        }

        /// <summary>
        /// mouse wheel / trackpad scroll gesture over the element. ctrl+wheel (which is also how
        /// chromium reports trackpad pinch) has its default page zoom suppressed, check
        /// WheelEventArgs.CtrlKey to implement zooming
        /// </summary>
        public event EventHandler<WheelEventArgs> Wheel
        {
            add
            {
                _wheel += value;
                AddToActiveEvents(JsCommonEvent.Wheel, _wheel);
            }
            remove
            {
                _wheel -= value;
                RemoveFromActiveEvents(JsCommonEvent.Wheel, _wheel);
            }
        }

        public event EventHandler SizeChanged
        {
            add
            {
                _sizeChanged += value;
                ObserveSize = true;
            }
            remove
            {
                _sizeChanged -= value;
                if (_sizeChanged == null || _sizeChanged.GetInvocationList().Length == 0)
                {
                    ObserveSize = false;
                }
            }
        }

        protected bool ObserveSize
        {
            get => _observeSize;
            set
            {
                if (_observeSize != value)
                {
                    if (Root != null)
                    {
                        if (value)
                        {
                            Root.ObserveList.Add(this as WView);
                        }
                        else
                        {
                            Root.UnobserveList.Add(this as WView);
                        }
                    }
                    _observeSize = value;
                }
            }
        }

        #region Measured
        public double Width { get; private set; }
        public double Height { get; private set; }

        public double X { get; private set; }
        public double Y { get; private set; }

        protected double _scrollX;
        protected double _scrollY; 
        protected double _scrollWidth;
        protected double _scrollHeight;
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InvokeEventOnMainThread<T>(EventHandler<T> handler, T args)
        {
            MainThread.BeginInvokeOnMainThread(() => handler?.Invoke(this, args));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InvokeEventOnMainThread(EventHandler handler, EventArgs args)
        {
            MainThread.BeginInvokeOnMainThread(() => handler?.Invoke(this, args));
        }

        public async Task WaitForNextUpdate()
        {
            var resume = new TaskCompletionSource();
            _nextTaskCompletion.Add(resume);
            try
            {
                await resume.Task.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch
            {
                //timeout
            }
        }

        internal void InvokeEvent(JsCommonEvent jsEvent, ReadOnlySpan<char> args, MemoryExtensions.SpanSplitEnumerator<char> argRanges)
        {           
            //all events send position of the element
            //argRanges.MoveNext();
            this.X = double.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value), CultureInfo.InvariantCulture);
            argRanges.MoveNext();
            this.Y = double.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value), CultureInfo.InvariantCulture);
            argRanges.MoveNext();
            this.Width = double.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value), CultureInfo.InvariantCulture);
            argRanges.MoveNext();
            this.Height = double.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value), CultureInfo.InvariantCulture);
            argRanges.MoveNext();
            _scrollX = double.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value), CultureInfo.InvariantCulture);
            argRanges.MoveNext();
            _scrollY = double.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value), CultureInfo.InvariantCulture);
            argRanges.MoveNext();
            _scrollWidth = double.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value), CultureInfo.InvariantCulture);
            argRanges.MoveNext();
            _scrollHeight = double.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value), CultureInfo.InvariantCulture);
            argRanges.MoveNext();

            //a disabled view drops user-input events here: pointer-events:none already blocks
            //most of them in the dom, this catches what css can't (keyboard on an already-focused
            //input, messages that crossed the bridge mid-toggle). scroll and resize still flow,
            //geometry bookkeeping like WVirtualList depends on them
            if (this is WView { IsEnabled: false }
                && jsEvent != JsCommonEvent.Scroll && jsEvent != JsCommonEvent.Resize)
            {
                CompleteNextTasks();
                return;
            }

            switch (jsEvent)
            {
                case JsCommonEvent.Click:
                    InvokeEventOnMainThread(_click, PointerEventArgs.ReadJsArgs(args, ref argRanges));
                    break;
                case JsCommonEvent.Dblclick:
                    InvokeEventOnMainThread(_dblclick, PointerEventArgs.ReadJsArgs(args, ref argRanges));
                    break;
                case JsCommonEvent.Pointerdown:
                    InvokeEventOnMainThread(_pointerdown, PointerEventArgs.ReadJsArgs(args, ref argRanges));
                    break;
                case JsCommonEvent.Pointermove:
                    InvokeEventOnMainThread(_pointermove, PointerEventArgs.ReadJsArgs(args, ref argRanges));
                    break;
                case JsCommonEvent.Pointerup:
                    InvokeEventOnMainThread(_pointerup, PointerEventArgs.ReadJsArgs(args, ref argRanges));
                    break;
                case JsCommonEvent.Pointerenter:
                    InvokeEventOnMainThread(_pointerenter, PointerEventArgs.ReadJsArgs(args, ref argRanges));
                    break;
                case JsCommonEvent.Pointerleave:
                    InvokeEventOnMainThread(_pointerleave, PointerEventArgs.ReadJsArgs(args, ref argRanges));
                    break;
                case JsCommonEvent.Pointerover:
                    InvokeEventOnMainThread(_pointerover, PointerEventArgs.ReadJsArgs(args, ref argRanges));
                    break;
                case JsCommonEvent.Pointerout:
                    InvokeEventOnMainThread(_pointerout, PointerEventArgs.ReadJsArgs(args, ref argRanges));
                    break;
                case JsCommonEvent.Pointercancel:
                    InvokeEventOnMainThread(_pointercancel, PointerEventArgs.ReadJsArgs(args, ref argRanges));
                    break;
                case JsCommonEvent.Keydown:
                    InvokeEventOnMainThread(_keydown, KeyEventArgs.ReadJsArgs(args, ref argRanges));
                    break;
                case JsCommonEvent.Keyup:
                    InvokeEventOnMainThread(_keyup, KeyEventArgs.ReadJsArgs(args, ref argRanges));
                    break;
                case JsCommonEvent.Focus:
                    InvokeEventOnMainThread(_focus, EventArgs.Empty);
                    break;
                case JsCommonEvent.Blur:
                    InvokeEventOnMainThread(_blur, EventArgs.Empty);
                    break;
                case JsCommonEvent.Scroll:
                    InvokeEventOnMainThread(_scroll, EventArgs.Empty);
                    break;
                case JsCommonEvent.Input:
                    InvokeEventOnMainThread(_input, InputEventArgs.ReadJsArgs(args, ref argRanges));
                    break;
                case JsCommonEvent.Change:
                    InvokeEventOnMainThread(_change, InputEventArgs.ReadJsArgs(args, ref argRanges));
                    break;
                case JsCommonEvent.Selectionchange:
                    InvokeEventOnMainThread(_selectionchange, SelectionChangeEventArgs.ReadJsArgs(args, ref argRanges));
                    break;
                case JsCommonEvent.Resize:
                    InvokeEventOnMainThread(_sizeChanged, EventArgs.Empty);
                    break;
                case JsCommonEvent.Wheel:
                    InvokeEventOnMainThread(_wheel, WheelEventArgs.ReadJsArgs(args, ref argRanges));
                    break;
            }

            CompleteNextTasks();
        }

        private void CompleteNextTasks()
        {
            if (_nextTaskCompletion.Count != 0)
            {
                foreach (var task in _nextTaskCompletion)
                {
                    task.TrySetResult();
                }
                _nextTaskCompletion.Clear();
            }
        }

        internal void InvokeReturn(int type, ReadOnlySpan<char> args, MemoryExtensions.SpanSplitEnumerator<char> argRanges)
        {
            //argRanges.MoveNext();
            this.X = double.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value), CultureInfo.InvariantCulture);
            argRanges.MoveNext();
            this.Y = double.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value), CultureInfo.InvariantCulture);
            argRanges.MoveNext();
            this.Width = double.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value), CultureInfo.InvariantCulture);
            argRanges.MoveNext();
            this.Height = double.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value), CultureInfo.InvariantCulture);
            argRanges.MoveNext();
            _scrollX = double.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value), CultureInfo.InvariantCulture);
            argRanges.MoveNext();
            _scrollY = double.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value), CultureInfo.InvariantCulture);
            argRanges.MoveNext();
            _scrollWidth = double.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value), CultureInfo.InvariantCulture);
            argRanges.MoveNext();
            _scrollHeight = double.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value), CultureInfo.InvariantCulture);


            if (_nextTaskCompletion.Count != 0)
            {
                foreach (var task in _nextTaskCompletion)
                {
                    task.TrySetResult();
                }
                _nextTaskCompletion.Clear();
            }
        }
    }
}
