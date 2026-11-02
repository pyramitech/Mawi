using Mawi.Helpers;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Views
{
    public class WEntry : WEntryBase
    {
        public WEntry() {

        }

        private bool _isPassword;
        private WKeyboard _keyboard;

        public bool IsPassword { get => _isPassword; set { _isPassword = value; MarkUpdated(); } }
        public WKeyboard Keyboard { get => _keyboard; set { _keyboard = value; Invalidate(); } }

        private string ToInputMode(WKeyboard keyboard) => keyboard switch
        {
            WKeyboard.Default => "text",
            WKeyboard.Text => "text",
            WKeyboard.Numeric => "numeric",
            WKeyboard.Telephone => "tel",
            WKeyboard.Url => "url",
            WKeyboard.Email => "email",
            WKeyboard.Decimal => "decimal",
            WKeyboard.Chat => "text",
            WKeyboard.Search => "search",
            _ => "text"
        };

        public override void Render(StringBuilder sb)
        {
            UpdateState = UpdateState.None;

            sb.Append("<input");
            RenderInternals(sb);
            RenderEvents(sb);
            sb.Append(" style=\\\"");
            RenderStyle(sb);
            sb.Append("\\\"");

            sb.Append(" type=");
            if (_isPassword)
            {
                sb.Append("\\\"password\\\"");
            }
            else
            {
                sb.Append("\\\"text\\\"");
            }

            if (_keyboard != WKeyboard.Default)
            {
                sb.Append(" inputmode=\\\"");
                sb.Append(ToInputMode(_keyboard));
                sb.Append("\\\"");
            }

            RenderTextSettings(sb);

            sb.Append("/>");
        }
    }

    public abstract class WEntryBase : WTextBase
    {
        public WEntryBase()
        {
            TextChanged += (s, e) => {
                _text = e.Value;
                _selectionIndex = e.SelectionStart;
                _selectionLength = e.SelectionEnd - e.SelectionStart;
            };
            SelectionChanged += (s, e) =>
            {
                _selectionIndex = e.SelectionStart;
                _selectionLength = e.SelectionEnd - e.SelectionStart;
            };
        }

        private ReturnType _returnType;

        private bool _isReadOnly;
        private bool _isSpellCheckEnabled;
        private bool _isTextPredictionEnabled;
        private int _maxLength = -1;
        private string _placeholder;
        private Color _placeholderColor;
        private int _selectionIndex = 0;
        private int _selectionLength = 0;

        private string _text;

        public ReturnType ReturnType { get => _returnType; set { _returnType = value; MarkUpdated(); } }


        public bool IsReadOnly { get => _isReadOnly; set { _isReadOnly = value; MarkUpdated(); } }
        public bool IsSpellCheckEnabled { get => _isSpellCheckEnabled; set { _isSpellCheckEnabled = value; MarkUpdated(); } }
        public bool IsTextPredictionEnabled { get => _isTextPredictionEnabled; set { _isTextPredictionEnabled = value; MarkUpdated(); } }
        public int MaxLength { get => _maxLength; set { _maxLength = value; MarkUpdated(); } }
        public string Placeholder { get => _placeholder; set { _placeholder = value; MarkUpdated(); } }

        public int SelectionIndex => _selectionIndex;
        public int SelectionLength => _selectionLength;

        public string Text { get => _text; set { _text = value;
                //a full render (Dirty covers New too) writes the value itself
                if ((this.UpdateState & UpdateState.Dirty) != UpdateState.Dirty)
                {
                    MarkDirty(UpdateState.DirtyValue);
                }
                _selectionIndex = _text?.Length ?? 0;
                _selectionLength = 0;
                if (_input != null)
                {
                    _input.Invoke(this, new InputEventArgs() { SelectionStart = _selectionIndex, SelectionEnd = _selectionIndex, Value = _text });
                }
            } 
        }

        public Color PlaceholderColor { get => _placeholderColor; set { _placeholderColor = value; MarkUpdated(); } }

        public event EventHandler<InputEventArgs> Completed
        {
            add
            {
                _change += value;
                AddToActiveEvents(JsCommonEvent.Change, _change);
            }
            remove
            {
                _change -= value;
                RemoveFromActiveEvents(JsCommonEvent.Change, _change);
            }
        }

        public event EventHandler<InputEventArgs> TextChanged
        {
            add
            {
                _input += value;
                AddToActiveEvents(JsCommonEvent.Input, _input);
            }
            remove
            {
                _input -= value;
                RemoveFromActiveEvents(JsCommonEvent.Input, _input);
            }
        }

        public event EventHandler<SelectionChangeEventArgs> SelectionChanged
        {
            add
            {
                _selectionchange += value;
                AddToActiveEvents(JsCommonEvent.Selectionchange, _selectionchange);
            }
            remove
            {
                _selectionchange -= value;
                RemoveFromActiveEvents(JsCommonEvent.Selectionchange, _selectionchange);
            }
        }

        public void SetSelection(int index)
        {
            Root.Execute($"setSelection(\"w{Id}\", {index}, {index});");
        }

        public void SetSelection(int start, int end)
        {
            Root.Execute($"setSelection(\"w{Id}\", {start}, {end});");
        }

        protected void RenderTextSettings(StringBuilder sb, bool renderValue = true)
        {
            if (_returnType != ReturnType.Default)
            {
                sb.Append(" enterkeyhint=");
                switch (_returnType)
                {
                    case ReturnType.Done:
                        sb.Append("\\\"done\\\"");
                        break;

                    case ReturnType.Go:
                        sb.Append("\\\"go\\\"");
                        break;

                    case ReturnType.Next:
                        sb.Append("\\\"next\\\"");
                        break;

                    case ReturnType.Search:
                        sb.Append("\\\"search\\\"");
                        break;

                    case ReturnType.Send:
                        sb.Append("\\\"send\\\"");
                        break;
                }
            }

            if (_isReadOnly)
            {
                sb.Append(" readonly");
            }

            if (_isSpellCheckEnabled)
            {
                sb.Append(" spellcheck=\\\"true\\\"");
            }

            if (_isTextPredictionEnabled)
            {
                sb.Append(" autocomplete=\\\"on\\\"");
            }

            if (_maxLength != -1)
            {
                sb.Append(" maxlength=\\\"").Append(_maxLength).Append("\\\"");
            }

            if (!string.IsNullOrEmpty(_placeholder))
            {
                sb.Append(" placeholder=\\\"").Append(_placeholder.MakeAttributeSafe()).Append("\\\"");
            }

            if (renderValue && !string.IsNullOrEmpty(_text))
            {
                sb.Append(" value=\\\"").Append(_text.MakeAttributeSafe()).Append("\\\"");
            }
        }

        public override void RenderChildLayout(WView child, StringBuilder sb)
        {
            //cannot have children
            throw new NotImplementedException();
        }

        public override void RenderStyle(StringBuilder sb)
        {
            RenderBackground(sb);
            RenderPosition(sb);
            RenderTransform(sb);
            RenderBorder(sb);
            RenderFont(sb);

            if (_placeholderColor != null)
            {
                sb.Append("--placeholder-color:").Append(_placeholderColor.ToRgbaHex(true)).Append(';');
            }

            Parent.RenderChildLayout(this, sb);
        }
    }

    public enum WKeyboard
    {
        Default,
        Text,
        Numeric,
        Telephone,
        Url,
        Email,
        Decimal,
        Chat,
        Search
    }
}
