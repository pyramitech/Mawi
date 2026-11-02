using Mawi.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Views
{
    [ContentProperty(nameof(Content))]
    public class WNavigationView : WView, ICustomContainer
    {
        public Stack<NavigationEntry> History = new(16);

        public WAnimation DefaultMountAnimation = null;
        public WAnimation DefaultUnmountAnimation = null;
        public WAnimation DefaultRemountAnimation = null;

        private WView _currentView;
        private WView _unloadingView;

        private EventHandler<WView> _navigated;
        public event EventHandler<WView> Navigated
        {
            add
            {
                _navigated += value;
            }
            remove
            {
                _navigated -= value;
            }
        }

        public WNavigationView()
        {
            VerticalOptions = LayoutOptions.Fill;
            HorizontalOptions = LayoutOptions.Fill;
        }

        public WView Content
        {
            get => _currentView;
            set
            {
                if (_currentView != null || Root != null)
                {
                    throw new Exception("Content can only be set directly at construction. Use PushNavigation otherwise.");
                }

                MarkDirty(UpdateState.Dirty);

                History.Push(new EmptyNavigationEntry
                {
                    MaintainState = true,
                    ViewType = typeof(WView),
                    View = value
                });

                _currentView = value;
                _currentView.Parent = this;
                if (_navigated != null)
                    _navigated.Invoke(this, _currentView);
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (_currentView != null)
            {
                SetInheritedBindingContext(_currentView, BindingContext);
            }
        }

        public async ValueTask PushNavigation<T>(
            TimeSpan animationDelay,
            bool maintainState = true,
            T view = null,
            WAnimation mountAnimation = null, 
            WAnimation unmountAnimation = null,
            WAnimation remountAnimation = null,
            bool clearHistory = false) where T : WView, new()
        {
            if (view == null)
            {
                view = new T();
            }
            if (mountAnimation == null)
            {
                mountAnimation = DefaultMountAnimation;
            }
            if (unmountAnimation == null)
            {
                unmountAnimation = DefaultUnmountAnimation;
            }
            if (remountAnimation == null)
            {
                remountAnimation = DefaultRemountAnimation;
            }

            _currentView = view;
            _currentView.Parent = this;
            if (BindingContext != null)
            {
                SetInheritedBindingContext(_currentView, BindingContext);
            }
            if (this.Root == null)
            {
                History.Push(new NavigationEntry<T>()
                {
                    MaintainState = maintainState,
                    View = view,
                    ViewType = typeof(T),
                    MountAnimation = mountAnimation,
                    UnmountAnimation = unmountAnimation,
                    RemountAnimation = remountAnimation
                });
                if (_unloadingView != null)
                {
                    _unloadingView.Parent = null;
                    _unloadingView = null;
                }
                return;
            }

            _currentView.AssignRoot(this.Root);

            MarkDirty(UpdateState.Dirty);

            var tasks = new List<Task>(2);

            NavigationEntry existing = null;

            if (History.Count != 0)
            {
                existing = History.Last();
                _unloadingView = existing.View;
                if (clearHistory)
                {   
                    History.Clear();
                }
            }

            History.Push(new NavigationEntry<T>() { 
                MaintainState = maintainState,
                View = view,
                ViewType = typeof(T),
                MountAnimation = mountAnimation,
                UnmountAnimation = unmountAnimation,
                RemountAnimation = remountAnimation
            });

            await Task.Delay(animationDelay);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (mountAnimation != null)
                {
                    tasks.Add(view.AnimateAsync(mountAnimation));
                }

                if (existing != null && unmountAnimation != null)
                {
                    tasks.Add(existing.View.AnimateAsync(unmountAnimation));
                }

                if (tasks.Count != 0)
                {
                    await Task.WhenAll(tasks);
                }
            });

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (existing != null)
                {
                    if (!existing.MaintainState)
                    {
                        existing.View = null;
                    }
                    Root.RemoveView(_unloadingView);
                    _unloadingView.UpdateState = UpdateState.Removed;
                    _unloadingView.Parent = null;
                    _unloadingView = null;
                }

                if (_navigated != null)
                    _navigated.Invoke(this, _currentView);
            });
        }

        public async ValueTask PushNavigation<T>(
            bool maintainState = true,
            T view = null,
            WAnimation mountAnimation = null,
            WAnimation unmountAnimation = null,
            WAnimation remountAnimation = null,
            bool clearHistory = false) where T : WView, new()
        {
            if (view == null)
            {
                view = new T();
            }
            if (mountAnimation == null)
            {
                mountAnimation = DefaultMountAnimation;
            }
            if (unmountAnimation == null)
            {
                unmountAnimation = DefaultUnmountAnimation;
            }
            if (remountAnimation == null)
            {
                remountAnimation = DefaultRemountAnimation;
            }

            _unloadingView = _currentView;

            _currentView = view;
            _currentView.Parent = this;
            if (BindingContext != null)
            {
                SetInheritedBindingContext(_currentView, BindingContext);
            }

            if (this.Root == null)
            {
                History.Push(new NavigationEntry<T>()
                {
                    MaintainState = maintainState,
                    View = view,
                    ViewType = typeof(T),
                    MountAnimation = mountAnimation,
                    UnmountAnimation = unmountAnimation,
                    RemountAnimation = remountAnimation
                });
                if (_unloadingView != null)
                {
                    _unloadingView.Parent = null;
                    _unloadingView = null;
                }
                return;
            }

            _currentView.AssignRoot(this.Root);

            MarkDirty(UpdateState.DirtyAdd);

            var tasks = new List<Task>(2);

            NavigationEntry existing = null;

            if (History.Count != 0)
            {
                existing = History.Last();
                _unloadingView = existing.View;
                if (clearHistory)
                {
                    History.Clear();
                }
            }

            History.Push(new NavigationEntry<T>()
            {
                MaintainState = maintainState,
                View = view,
                ViewType = typeof(T),
                MountAnimation = mountAnimation,
                UnmountAnimation = unmountAnimation,
                RemountAnimation = remountAnimation
            });

       
            if (mountAnimation != null)
            {
                tasks.Add(view.AnimateAsync(mountAnimation));
            }

            if (existing != null && unmountAnimation != null)
            {
                tasks.Add(existing.View.AnimateAsync(unmountAnimation));
            }

            if (tasks.Count != 0)
            {
                await Task.WhenAll(tasks);
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (existing != null)
                {
                    if (!existing.MaintainState)
                    {
                        existing.View = null;
                    }
                    Root.RemoveView(_unloadingView);
                    _unloadingView.UpdateState = UpdateState.Removed;
                    _unloadingView.Parent = null;
                    _unloadingView = null;
                }
            });

            if (_navigated != null)
                _navigated.Invoke(this, _currentView);
        }

        public async ValueTask PopNavigation()
        {
            if (History.Count < 2)
            {
                return;
            }

            var curHistory = History.Pop();
            var history = History.Peek();

            _unloadingView = _currentView;

            if (history.MaintainState)
            {
                _currentView = history.View;
            }
            else
            {
                _currentView = Activator.CreateInstance(history.ViewType) as WView;
            }

            _currentView.Parent = this;
            if (BindingContext != null)
            {
                SetInheritedBindingContext(_currentView, BindingContext);
            }

            if (this.Root == null)
            {
                _unloadingView.UpdateState = UpdateState.Removed;
                _unloadingView.Parent = null;
                _unloadingView = null;
                return;
            }
            _currentView.AssignRoot(this.Root);

            MarkDirty(UpdateState.DirtyAdd);

            var tasks = new List<Task>(2);

            if (curHistory.UnmountAnimation != null)
            {
                tasks.Add(_unloadingView.AnimateAsync(curHistory.UnmountAnimation));
            }

            if (history.RemountAnimation != null)
            {
                tasks.Add(_currentView.AnimateAsync(history.RemountAnimation));
            }

            if (tasks.Count != 0)
            {
                await Task.WhenAll(tasks);
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {                
                Root.RemoveView(_unloadingView);
                _unloadingView.UpdateState = UpdateState.Removed;
                _unloadingView.Parent = null;
                _unloadingView = null;                
            });

            if (_navigated != null)
                _navigated.Invoke(this, _currentView);
        }

        public override void Render(StringBuilder sb)
        {
            this.UpdateState = UpdateState.None;

            sb.Append("<w-nav");
            RenderInternals(sb);
            RenderEvents(sb);
            sb.Append(" style=\\\"");
            RenderStyle(sb);
            sb.Append("\\\">");
            if (_unloadingView != null)
            {
                _unloadingView.Render(sb);
            }
            if (_currentView != null)
            {
                _currentView.Render(sb);
            }
            sb.Append("</w-nav>");
        }

        public void RenderNewViews(StringBuilder sb)
        {
            //if (_currentView?.UpdateState == UpdateState.New)
            if (_currentView != null)
            {
                _currentView.Render(sb);
            }
        }

        public override void RenderChildLayout(WView child, StringBuilder sb)
        {
            sb.Append("justify-self: stretch; align-self:stretch; grid-row: 1; grid-column: 1;");
            //sb.Append("justify-self: ");
            //switch (child.HorizontalOptions.Alignment)
            //{
            //    case LayoutAlignment.Fill:
            //        if (child.WidthRequest == -1)
            //        {
            //            sb.Append("stretch");
            //        }
            //        else
            //        {
            //            sb.Append("center");
            //        }
            //        break;

            //    case LayoutAlignment.Start:
            //        sb.Append("start");
            //        break;

            //    case LayoutAlignment.Center:
            //        sb.Append("center");
            //        break;

            //    case LayoutAlignment.End:
            //        sb.Append("end");
            //        break;
            //}
            //sb.Append(";");

            //sb.Append("align-self:");
            //switch (child.VerticalOptions.Alignment)
            //{
            //    case LayoutAlignment.Fill:
            //        if (child.HeightRequest == -1)
            //        {
            //            sb.Append("stretch");
            //        }
            //        else
            //        {
            //            sb.Append("center");
            //        }
            //        break;

            //    case LayoutAlignment.Start:
            //        sb.Append("start");
            //        break;

            //    case LayoutAlignment.Center:
            //        sb.Append("center");
            //        break;

            //    case LayoutAlignment.End:
            //        sb.Append("end");
            //        break;
            //}
            //sb.Append(";");
        }

        public override void RenderStyle(StringBuilder sb)
        {
            sb.Append("display:grid;");

            RenderBackground(sb);
            RenderPosition(sb);
            RenderTransform(sb);
            RenderBorder(sb);

            sb.Append("grid-template-rows:1fr;");
            sb.Append("grid-template-columns:1fr;");

            Parent.RenderChildLayout(this, sb);
        }

        internal override void AssignRoot(WRoot root)
        {
            base.AssignRoot(root);
            foreach (var history in History)
            {
                if (history.View != null)
                {
                    history.View.AssignRoot(root);
                }
            }

            if (_navigated != null)
                _navigated.Invoke(this, _currentView);
            //if (_currentView != null) {
            //    _currentView.AssignRoot(root);
            //}
            //if (_unloadingView != null)
            //{
            //    _unloadingView.AssignRoot(root);
            //}
        }

        internal override void UnassignRoot()
        {
            base.UnassignRoot();

            foreach (var history in History)
            {
                if (history.View != null)
                {
                    history.View.UnassignRoot();
                }
            }
            //if (_currentView != null)
            //{
            //    _currentView.UnassignRoot();
            //}
            //if (_unloadingView != null)
            //{
            //    _unloadingView.UnassignRoot();
            //}
        }

        public abstract class NavigationEntry
        {
            public bool MaintainState;
            public Type ViewType;
            public WView View;
            public WAnimation MountAnimation;
            public WAnimation UnmountAnimation;
            public WAnimation RemountAnimation;

            public abstract WView Create();
        }

        /// <summary>
        /// Only used when inserting from Content
        /// </summary>
        private sealed class EmptyNavigationEntry : NavigationEntry
        {
            public override WView Create()
            {
                throw new NotImplementedException();
            }
        }

        public sealed class NavigationEntry<T> : NavigationEntry where T : WView, new()
        {
            public override WView Create()
            {
                return new T();
            }
        }
    }
}
