using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Views
{
    [ContentProperty(nameof(Children))]
    public abstract class WContainer : WView
    {
        public WObservableCollection<WView> Children { get; set; } = new();

        public WContainer()
        {
            Children.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
#if DEBUG
            if (!MainThread.IsMainThread)
            {
                WRoot.ErrorDebugAction?.Invoke(new InvalidOperationException(
                    $"{GetType().Name}.Children modified off the main thread, marshal the call with MainThread.BeginInvokeOnMainThread"));
            }
#endif
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    MarkDirty(UpdateState.DirtyAdd);
                    foreach (var item in e.NewItems)
                    {
                        var view = (item as WView);

                        view.Parent = this;
                        if (BindingContext != null)
                        {
                            SetInheritedBindingContext(view, BindingContext);
                        }
                        if (Root != null)
                        {
                            view.AssignRoot(this.Root);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        if (Root != null)
                        {
                            //if state is already Add
                            var view = (item as WView);
                            Root.RemoveView(view);
                            view.UpdateState = UpdateState.Removed;
                            //view.UnassignRoot();
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    //because it's getting replaced there's no reason to add anything to removelist
                    MarkDirty(UpdateState.Dirty);
                    if (this.Root != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            (item as WView).UnassignRoot();
                        }
                        foreach (var item in e.NewItems)
                        {
                            var view = (item as WView);
                            if (BindingContext != null)
                            {
                                SetInheritedBindingContext(view, BindingContext);
                            }
                            view.AssignRoot(this.Root);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    //idk if this needs a better solution and how often you move items within a collection
                    MarkDirty(UpdateState.Dirty);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    //do full reset
                    MarkDirty(UpdateState.Dirty);
                    break;
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            foreach (var child in Children)
            {
                SetInheritedBindingContext(child, BindingContext);
            }
        }

        internal override void AssignRoot(WRoot root)
        {
            base.AssignRoot(root);
            foreach (var child in this.Children)
            {
                child.AssignRoot(root);
            }
        }

        internal override void UnassignRoot()
        {
            base.UnassignRoot();
            foreach (var child in this.Children)
            {
                child.UnassignRoot();
            }
        }
    }

    public interface ICustomContainer
    {
        void RenderNewViews(StringBuilder sb);
    }

    /// <summary>
    /// a custom container that can also insert new views at the start of its element
    /// </summary>
    public interface IPrependContainer
    {
        bool HasPrependViews { get; }
        void RenderPrependViews(StringBuilder sb);
    }

    public class WObservableCollection<T> : ObservableCollection<T>
    {
        private static List<byte> _empty = new(); //an empty list
        protected override void ClearItems()
        {
            var removed = this.ToList();

            base.ClearItems();

            if (removed.Count > 0)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace, _empty, removed, -1));
            }
        }
    }
}
