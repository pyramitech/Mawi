using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Views
{
    [ContentProperty(nameof(Content))]
    public abstract class WContent : WView
    {
        protected WView _content;
        public WView Content { get => _content;
            set {
                MarkDirty(UpdateState.Dirty);
                if (_content != null)
                {
                    _content.UpdateState = UpdateState.Removed;

                    if (this.Root != null)
                    {
                        Root.RemoveView(_content);
                    }
                }

                _content = value;
                if (value != null)
                {
                    _content.UpdateState = UpdateState.New;
                    _content.Parent = this;
                    if (BindingContext != null)
                    {
                        SetInheritedBindingContext(_content, BindingContext);
                    }
                    if (this.Root != null)
                    {
                        _content.AssignRoot(this.Root);
                    }
                }
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (_content != null)
            {
                SetInheritedBindingContext(_content, BindingContext);
            }
        }

        internal override void AssignRoot(WRoot root)
        {
            base.AssignRoot(root);
            if (_content != null)
            {
                _content.AssignRoot(this.Root);
            }
        }

        internal override void UnassignRoot()
        {
            base.UnassignRoot();
            if (_content != null)
            {
                _content.UnassignRoot();
            }
        }
    }
}
