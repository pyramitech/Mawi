using Mawi.Animation;
using Mawi.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Views
{
    public interface IWRenderTarget
    {
        void Render(StringBuilder sb);

        void RenderChildLayout(WView child, StringBuilder sb);
    }   

    public abstract class WView : WElement, IWRenderTarget
    {
        private List<string> _execWaitList = null;

        private LayoutOptions _horizontalOptions = LayoutOptions.Fill;
        private LayoutOptions _verticalOptions = LayoutOptions.Fill;
        private WShadowList _shadow = null;
        private WFilterList _backdrop = null;
        private WFilterList _filter = null;
        private bool _isEnabled = true;
        private bool _isFocused = false;
        private bool _isVisible = true;
        private double _opacity = 1.0;
        private Brush _background = Brush.Default;
        private Color _backgroundColor;
        private WLength _minimumWidth = -1;
        private WLength _maximumWidth = double.PositiveInfinity;
        private WLength _minimumHeight = -1;
        private WLength _maximumHeight = double.PositiveInfinity;

        private WLength _requestWidth = -1;
        private WLength _requestHeight = -1;

        private WThickness _margin;
        private WThickness _padding;
        private int _zIndex;
        private bool _inputTransparent = false;
        private WPoint _translation = 0;
        private Point3 _scale = 1;
        private double _rotation = 0;

        private IWLayoutSettings _layoutSettings;

        private WThickness _borderRadius;
        private Color _borderColor = Colors.Transparent;
        private WThickness _borderWidth;
        private CssBorderStyle _borderStyle;

        private bool _preventFocus;

        protected bool _isColorTransparent = true;

        private ImmutableList<string> _styleClasses = null;

        public ImmutableList<string> StyleClasses { get => _styleClasses; set {
                _styleClasses = value;
                MarkUpdated();
            } 
        }

        /// <summary>
        /// This function will prevent click events from stealing focus, however for it to work you must attach a "pointerdown" event 
        /// </summary>
        public bool PreventFocus { 
            get => _preventFocus; 
            set
            {
                _preventFocus = value;
                Invalidate();
            }
        }

        public IWLayoutSettings LayoutSettings { get => _layoutSettings; set { _layoutSettings = value; _layoutSettings.AssignParent(this); MarkUpdated(); } }

        public LayoutOptions HorizontalOptions { get => _horizontalOptions; set { _horizontalOptions = value; MarkUpdated(); } }
        public LayoutOptions VerticalOptions { get => _verticalOptions; set { _verticalOptions = value; MarkUpdated(); } }
        [CssName("boxShadow")]
        public WShadowList Shadow { get => _shadow; set { _shadow = value; MarkUpdated(); } }
        [CssName("backdropFilter")]
        public WFilterList Backdrop { get => _backdrop; set { _backdrop = value; MarkUpdated(); } }
        [CssName("filter")]
        public WFilterList Filter { get => _filter; set { _filter = value; MarkUpdated(); } }
        /// <summary>
        /// disabled views get pointer-events:none plus a `disabled` attribute (real semantics on
        /// form elements, a [disabled] css hook everywhere), and any event that still gets
        /// through is dropped before handlers run. no automatic dimming: style it via [disabled]
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value)
                    return;
                _isEnabled = value;
                MarkUpdated();
                //the attribute only renders on a full re-render (which would wipe an input's live
                //text), so runtime toggles patch it directly. before root assignment the initial
                //render covers it
                Root?.Execute($"setEnabled(\"w{Id}\", {(value ? "true" : "false")});");
            }
        }
        public bool IsFocused { get => _isFocused; private set { _isFocused = value; MarkUpdated(); } }
        public bool IsVisible { get => _isVisible; set { _isVisible = value; MarkUpdated(); } }

        /// <summary>
        /// Border radius (in pixels), corresponding to CSS "border-radius".
        /// </summary>
        public WThickness BorderRadius { get => _borderRadius; set { _borderRadius = value; MarkUpdated(); } }

        /// <summary>
        /// Border color, corresponding to CSS "border-color".
        /// </summary>
        public Color BorderColor { get => _borderColor; set { _borderColor = value; MarkUpdated(); } }

        /// <summary>
        /// Border width, corresponding to CSS "border-width".
        /// </summary>
        public WThickness BorderWidth { get => _borderWidth; set { _borderWidth = value; MarkUpdated(); } }

        /// <summary>
        /// Border style, corresponding to CSS "border-style".
        /// </summary>
        public CssBorderStyle BorderStyle { get => _borderStyle; set { _borderStyle = value; MarkUpdated(); } }

        [CssName("opacity")]
        public double Opacity { get => _opacity; set { _opacity = value; MarkUpdated(); } }

        [CssName("background")]
        public Brush Background { get => _background; set { 
                _background = value; 
                if (_background == null)
                {
                    _isColorTransparent = (_backgroundColor == null || _backgroundColor.Alpha == 0);                    
                }
                else
                {
                    if (_background is SolidColorBrush scb && scb.Color != null)
                    {
                        _isColorTransparent = scb.Color.Alpha == 0;
                    }
                    else
                    {
                        _isColorTransparent = false;
                    }
                }
                MarkUpdated(); 
            } 
        }

        [CssName("backgroundColor")]
        public Color BackgroundColor { get => _backgroundColor; set 
            { 
                _backgroundColor = value;
                if (_background == null)
                {
                    _isColorTransparent = (_backgroundColor == null || _backgroundColor.Alpha == 0);
                }
                else
                {
                    if (_background is SolidColorBrush scb && scb.Color != null)
                    {
                        _isColorTransparent = scb.Color.Alpha == 0;
                    }
                    else
                    {
                        _isColorTransparent = false;
                    }
                }
                MarkUpdated(); 
            }
        }

        [CssName("minWidth")]
        public WLength MinimumWidth { get => _minimumWidth; set { _minimumWidth = value; MarkUpdated(); } }
        [CssName("maxWidth")]
        public WLength MaximumWidth { get => _maximumWidth; set { _maximumWidth = value; MarkUpdated(); } }
        [CssName("minHeight")]
        public WLength MinimumHeight { get => _minimumHeight; set { _minimumHeight = value; MarkUpdated(); } }
        [CssName("maxHeight")]
        public WLength MaximumHeight { get => _maximumHeight; set { _maximumHeight = value; MarkUpdated(); } }

        [CssName("width")]
        public WLength WidthRequest { get => _requestWidth; set { _requestWidth = value; MarkUpdated(); } }
        [CssName("height")]
        public WLength HeightRequest { get => _requestHeight; set { _requestHeight = value; MarkUpdated(); } }

        [CssName("margin")]
        public WThickness Margin { get => _margin; set { _margin = value; MarkUpdated(); } }
        [CssName("padding")]
        public WThickness Padding { get => _padding; set { _padding = value; MarkUpdated(); } }
        [CssName("zIndex")]
        public int ZIndex { get => _zIndex; set { _zIndex = value; MarkUpdated(); } }
        public bool InputTransparent { get => _inputTransparent; set { _inputTransparent = value; MarkUpdated(); } }
        [CssName("translate")]
        public WPoint Translation { get => _translation; set { _translation = value; MarkUpdated(); } }
        public WLength TranslationX { get => _translation.X; set { _translation = new WPoint(value, _translation.Y); MarkUpdated(); } }
        public WLength TranslationY { get => _translation.Y; set { _translation = new WPoint(_translation.X, value); MarkUpdated(); } }
        [CssName("scale" )]
        public Point3 Scale { get => _scale; set { _scale = value; MarkUpdated(); } }
        [CssName("rotate", false, "deg")]
        public double Rotation { get => _rotation; set { _rotation = value; MarkUpdated(); } }
        //[CssName("rotateX", true, "deg")]
        //public double RotationX { get => _rotationX; set { _rotationX = value; MarkUpdated(); } }
        //[CssName("rotateY", true, "deg")]
        //public double RotationY { get => _rotationY; set { _rotationY = value; MarkUpdated(); } }


        public new IWRenderTarget Parent { get; internal set; }


        public void Invalidate() => MarkDirty(UpdateState.Dirty);

        public void MarkUpdated() => MarkDirty(UpdateState.DirtyStyle);

        public abstract void Render(StringBuilder sb);
        public abstract void RenderStyle(StringBuilder sb);
        public abstract void RenderChildLayout(WView child, StringBuilder sb);

        public void Measure()
        {
            var cmd = $"measure(\"w{Id}\");";
            if (Root != null)
            {
                Root.Execute(cmd);
            }
            else
            {
                if (_execWaitList == null)
                    _execWaitList = new();
                _execWaitList.Add(cmd);
            }
        }

        public void Focus()
        {
            var cmd = $"focus(\"w{Id}\");";
            if (Root != null)
            {
                Root.Execute(cmd);
            }
            else
            {
                if (_execWaitList == null)
                    _execWaitList = new();
                _execWaitList.Add(cmd);
            }
        }

        public void Unfocus()
        {
            var cmd = $"unfocus(\"w{Id}\");";
            if (Root != null)
            {
                Root.Execute(cmd);
            }
            else
            {
                if (_execWaitList == null)
                    _execWaitList = new();
                _execWaitList.Add(cmd);
            }
        }

        public void Animate(WAnimation anim)
        {
            Root.AnimateView((this, anim, Guid.Empty, null));
        }

        public async Task AnimateAsync(WAnimation anim)
        {
            if (Root == null)
                return;

            var guid = Guid.NewGuid();
            var resume = new TaskCompletionSource();
            Root.AnimationCompletion.TryAdd(guid, resume);
            Root.AnimateView((this, anim, guid, resume));
            try
            {
                await resume.Task.WaitAsync(TimeSpan.FromMilliseconds(anim.Duration + 1000)); //timeout after duration + 1 second anyway
            }
            catch (TimeoutException)
            {
                if (Root != null)
                {
                    Root.AnimationCompletion.TryRemove(guid, out var value);
                }
            }
        }

        public void CancelAnimations()
        {
            Root.CancelAnimateView(this);
        }

        public void ScrollIntoView(ScrollBehavior behavior = ScrollBehavior.Auto, 
            ScrollAlignment block = ScrollAlignment.Start,
            ScrollAlignment inline = ScrollAlignment.Nearest)
        {
            var type = behavior switch
            {
                ScrollBehavior.Instant => "instant",
                ScrollBehavior.Smooth => "smooth",
                _ => "auto",
            };

            var blockType = block switch {
                ScrollAlignment.Nearest => "nearest",
                ScrollAlignment.Center => "center",
                ScrollAlignment.End => "end",
                _ => "start"
            };

            var inlineType = block switch
            {
                ScrollAlignment.Start => "start",
                ScrollAlignment.Center => "center",
                ScrollAlignment.End => "end",
                _ => "nearest"
            };

            var cmd = $"scrollIntoView(\"w{Id}\", \"{type}\", \"{blockType}\", \"{inlineType}\");";
            if (Root != null)
            {
                Root.Execute(cmd);
            }
            else
            {
                if (_execWaitList == null)
                    _execWaitList = new();
                _execWaitList.Add(cmd);
            }
        }

        protected void RenderInternals(StringBuilder sb)
        {
            sb.Append(" id=\\\"w").Append(this.Id).Append("\\\" ");
            if (_styleClasses != null)
            {
                sb.Append(" class=\\\"");
                _styleClasses.ForEach(x => sb.Append(x.MakeAttributeSafe()).Append(' '));
                sb.Append("\\\"");
            }
            if (_preventFocus)
            {
                sb.Append(" data-no-focus ");
            }
            if (!_isEnabled)
            {
                sb.Append(" disabled ");
            }
        }

        public void RenderBorder(StringBuilder sb)
        {
            if (!_borderRadius.IsEmpty) { 
                sb.Append("border-radius:");
                _borderRadius.Write(sb);
                sb.Append(';');
            }
            if (!_borderWidth.IsEmpty)
            {
                sb.Append("border-width:");
                _borderWidth.Write(sb);
                sb.Append(';');

                _borderStyle.Write(sb);

                if (_borderColor != null)
                {
                    sb.Append("border-color:");
                    sb.Append(_borderColor.ToRgbaHex(true));
                    sb.Append(';');
                }
            }
        }

        public virtual void RenderPosition(StringBuilder sb)
        {
            if (!_isVisible)
            {
                sb.Append("display: none;");
            }

            if (!_margin.IsEmpty)
            {
                sb.Append("margin: ");
                _margin.Write(sb);
                sb.Append(';');
            }

            if (!_padding.IsEmpty)
            {
                sb.Append("padding: ");
                _padding.Write(sb);
                sb.Append(';');
            }

            if (_requestWidth.Value != -1)
            {                
                sb.Append("width: ");
                _requestWidth.Write(sb);
                sb.Append(';');                
            }

            if (_requestHeight.Value != -1)
            {
                sb.Append("height: ");
                _requestHeight.Write(sb);
                sb.Append(';');
            }

            if (_minimumWidth.Value != -1)
            {
                sb.Append("min-width: ");
                _minimumWidth.Write(sb);
                sb.Append(';');
            }

            if (_minimumHeight.Value != -1)
            {
                sb.Append("min-height: ");
                _minimumHeight.Write(sb);
                sb.Append(';');
            }

            if (_maximumWidth.Value != Double.PositiveInfinity)
            {
                sb.Append("max-width: ");
                _maximumWidth.Write(sb);
                sb.Append(';');
            }

            if (_maximumHeight.Value != Double.PositiveInfinity)
            {
                sb.Append("max-height: ");
                _maximumHeight.Write(sb);
                sb.Append(';');
            }

            if (ZIndex != 0)
            {
                sb.Append("z-index: ").Append(ZIndex).Append(";");
            }
        }

        public virtual void RenderBackground(StringBuilder sb)
        {
            if (BackgroundColor != null)
            {
                sb.Append("background-color:").Append(BackgroundColor.ToRgbaHex(true)).Append(';');
            }
            else if (Background != null)
            {
                if (Background is SolidColorBrush solidBrush)
                {
                    if (solidBrush.Color != null)
                        sb.Append("background:").Append(solidBrush.Color.ToRgbaHex(true)).Append(';');
                }
                else if (Background is LinearGradientBrush)
                {
                    var brush = Background as LinearGradientBrush;
                    var angle = Math.Atan2(brush.EndPoint.Y - brush.StartPoint.Y, brush.EndPoint.X - brush.StartPoint.X) * (180 / Math.PI);
                    if (angle < 0) //normalize
                    {
                        angle += 360;
                    }

                    sb.Append("background:linear-gradient(").Append(angle.ToInv()).Append("deg, ");

                    for (int i = 0; i < brush.GradientStops.Count; i++)
                    {
                        var stop = brush.GradientStops[i];
                        sb.Append(stop.Color.ToRgbaHex()).Append(' ').Append(stop.Offset * 100).Append('%');

                        if (i + 1 != brush.GradientStops.Count)
                        {
                            sb.Append(',');
                        }
                    }

                    sb.Append(");");
                }
                else if (Background is RadialGradientBrush)
                {
                    var brush = Background as RadialGradientBrush;

                    var gradientOriginX = brush.Center.X * 100;
                    var gradientOriginY = brush.Center.Y * 100;

                    var radius = brush.Radius * 100;

                    sb.Append("background:radial-gradient(").Append(radius.ToInv()).Append("% ").Append(radius.ToInv()).Append("% ellipse at ").Append((brush.Center.X * 100).ToInv()).Append("% ").Append((brush.Center.Y * 100).ToInv()).Append(", ");

                    for (int i = 0; i < brush.GradientStops.Count; i++)
                    {
                        //convert color to CSS rgba()
                        var stop = brush.GradientStops[i];
                        sb.Append(stop.Color.ToRgbaHex()).Append(' ').Append(stop.Offset * 100).Append('%');

                        if (i + 1 != brush.GradientStops.Count)
                        {
                            sb.Append(',');
                        }
                    }

                    sb.Append(");");
                }
            }

            if (_opacity != 1)
            {
                sb.Append("opacity: ").Append(_opacity.ToInv()).Append(';');
            }

            if (_inputTransparent || !_isEnabled) //|| (_isColorTransparent && _click == null && _dblclick == null))
            {
                sb.Append("pointer-events: none;");
            }

            if (_shadow != null && _shadow.Length != 0)
            {
                sb.Append("box-shadow: ");
                _shadow.Write(sb);
                sb.Append(';');
            }

            if (_backdrop != null && _backdrop.Length != 0)
            {
                sb.Append("backdrop-filter: ");
                _backdrop.Write(sb);
                sb.Append(';');

                //safari annoyance
                sb.Append("-webkit-backdrop-filter: ");
                _backdrop.Write(sb);
                sb.Append(';');
            }

            if (_filter != null && _filter.Length != 0)
            {
                sb.Append("filter: ");
                _filter.Write(sb);
                sb.Append(';');
            }
        }

        public virtual void RenderTransform(StringBuilder sb)
        {
            if (!_translation.IsEmpty)
            {
                sb.Append("translate: ");
                _translation.Write(sb);
                sb.Append(';');
            }

            if (_scale.X != 1 || _scale.Y != 1 || _scale.Z != 1)
            {
                sb.Append("scale: ").Append(_scale.X).Append(' ').Append(_scale.Y);
                if (_scale.Z != 1)
                {
                    sb.Append(' ').Append(_scale.Z);
                }
                sb.Append(';');
            }

            if (_rotation != 0)
            {
                sb.Append("rotate: ").Append(_rotation).Append("deg;");
            }
            //var hasTransform = false;
            //var writeTransform = () => { 
            //    if (!hasTransform)
            //    {
            //        hasTransform = true;
            //        sb.Append("transform:");
            //    }
            //};

            //if (_translationX.Value != 0)
            //{
            //    writeTransform();
            //    sb.Append(" translateX(").Append(_translationX).Append(")");
            //}

            //if (_translationY.Value != 0)
            //{
            //    writeTransform();
            //    sb.Append(" translateY(").Append(_translationY).Append(")");
            //}

            //if (_scale != 1)
            //{
            //    writeTransform();
            //    sb.Append(" scale(").Append(_scale.ToInv()).Append(")");
            //}

            //if (_scaleX != 1)
            //{
            //    writeTransform();
            //    sb.Append(" scaleX(").Append(_scaleX.ToInv()).Append(")");
            //}

            //if (_scaleY != 1)
            //{
            //    writeTransform();
            //    sb.Append(" scaleY(").Append(_scaleY.ToInv()).Append(")");
            //}

            //if (_rotation != 0)
            //{
            //    writeTransform();
            //    sb.Append(" rotateZ(").Append(_rotation.ToInv()).Append("deg)");
            //}

            //if (_rotationX != 0)
            //{
            //    writeTransform();
            //    sb.Append(" rotateX(").Append(_rotationX.ToInv()).Append("deg)");
            //}

            //if (_rotationY != 0)
            //{
            //    writeTransform();
            //    sb.Append(" rotateY(").Append(_rotationY.ToInv()).Append("deg)");
            //}


            //if (hasTransform)
            //{
            //    sb.Append(";");
            //}
        }

        internal override void AssignRoot(WRoot root)
        {
            this.Root = root;
            root.Views[this.Id] = this;
            if (_observeSize)
                root.ObserveList.Add(this);
            if (_execWaitList != null)
            {
                foreach (var cmd in _execWaitList)
                {
                    root.Execute(cmd);
                }
                _execWaitList = null;
            }
        }

        internal override void UnassignRoot()
        {
            //the js observer has to go with the element, but the intent must survive: ObserveSize
            //mirrors the SizeChanged subscription, so clearing it here would stop AssignRoot from
            //ever re-observing a view that gets re-attached later (a cached page navigated back
            //to, for instance), leaving it permanently without size notifications
            if (ObserveSize)
            {
                this.Root.UnobserveList.Add(this);
            }
            this.Root.Views.TryRemove(this.Id, out _);
            this.Root = null;
        }
    }

    public enum UpdateState
    {
        None = 0,
        DirtyStyle = 1, //updates only css style
        DirtyAdd = 2, //will add children that are marked with New state
        DirtyEvent = 4, //updates events
        Dirty = 7, //fully updates outerHTML
        New = 15,
        Removed = 16, //object is removed
        DirtyValue = 32, //updates the value of an input element
    }
}
