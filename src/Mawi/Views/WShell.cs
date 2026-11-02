using Mawi.Animation;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Views
{
    public class WShell : WView, ICustomContainer
    {
        public WAnimation DefaultMountAnimation = null;

        private Dictionary<string, WShellEntry> _pages = new Dictionary<string, WShellEntry>();

        private WView _currentView;
        private WView _unloadingView;

        public WView CurrentPage => _currentView;
        public string CurrentRoute { get; private set; }


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

        public WShell()
        {
            VerticalOptions = LayoutOptions.Fill;
            HorizontalOptions = LayoutOptions.Fill;
        }

        public void AddPage(string route, Func<WView> template)
        {
            _pages[route] = new WShellEntry() { Template = template };
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (_currentView != null)
            {
                SetInheritedBindingContext(_currentView, BindingContext);
            }
        }

        public async ValueTask NavigateTo(string route,
            WAnimation mountAnimation = null)
        {
            if (route == CurrentRoute)
                return;

            if (_pages.TryGetValue(route, out var page))
            {
                CurrentRoute = route;
                if (page.View == null)
                {
                    page.View = page.Template();
                }

                _unloadingView = _currentView;
                _currentView = page.View;
                _currentView.Parent = this;
                if (BindingContext != null)
                {
                    SetInheritedBindingContext(_currentView, BindingContext);
                }

                if (this.Root == null)
                {
                    if (_unloadingView != null)
                    {
                        _unloadingView.Parent = null;
                        _unloadingView = null;
                    }
                    return;
                }

                while (_currentView.Root != null)
                {
                    //the only way for this to happen is if this view
                    //was just navigated away, but not yet removed from root
                    //so idk the only way I can think of is just wait until
                    //it's properly removed
                    await Task.Delay(200);
                }

                _currentView.AssignRoot(this.Root);

                MarkDirty(UpdateState.Dirty);

                if (mountAnimation != null)
                {
                    await _currentView.AnimateAsync(mountAnimation);
                }
                else if (DefaultMountAnimation != null) 
                {
                    await _currentView.AnimateAsync(DefaultMountAnimation);
                }

                if (_unloadingView != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {                        
                        Root.RemoveView(_unloadingView);
                        _unloadingView.UpdateState = UpdateState.Removed;
                        _unloadingView.Parent = null;
                        _unloadingView = null;                        
                    });
                }
                if (_navigated != null)
                    _navigated.Invoke(this, _currentView);
            }
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
            if (_currentView != null)
            {
                _currentView.Render(sb);
            }
        }

        public override void RenderChildLayout(WView child, StringBuilder sb)
        {
            sb.Append("justify-self: stretch; align-self:stretch; grid-row: 1; grid-column: 1;");
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

        public WView GetPageInstance(string route)
        {
            if (_pages.TryGetValue(route, out var page))
            {
                return page.View;
            }
            return null;
        }

        internal override void AssignRoot(WRoot root)
        {
            base.AssignRoot(root);
            if (_currentView != null)
            {
                _currentView.AssignRoot(root);
                if (_navigated != null)
                    _navigated.Invoke(this, _currentView);
            }
        }

        internal override void UnassignRoot()
        {
            base.UnassignRoot();

            if (_currentView != null)
                _currentView.UnassignRoot();
        }

        private class WShellEntry
        {
            public WView View = null;
            public Func<WView> Template;
        }
    }
}
