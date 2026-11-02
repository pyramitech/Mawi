using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi
{
    public enum JsCommonEvent
    {
        Click,
        Dblclick,
        Pointerdown,
        Pointermove,
        Pointerup,
        Pointerenter,
        Pointerleave,
        Pointerover,
        Pointerout,
        Pointercancel,
        Keydown,
        Keyup,
        Input,
        Change,
        Focus,
        Blur,
        Submit,
        Scroll = 17,
        Selectionchange,
        Resize,
        Wheel
    }

    public enum JsCallFunctions
    {
        Measure,
        Focus,
        Unfocus,
        SetValue,
        SetSelection,
        ScrollTo,
        ScrollIntoView
    }

    public class KeyEventArgs
    {
        public string Key { get; set; }
        public bool AltKey { get; set; }
        public bool CtrlKey { get; set; }
        public bool ShiftKey { get; set; }
        public bool MetaKey { get; set; }
        public bool Repeat { get; set; }
        public int Location { get; set; }
        public bool IsComposing { get; set; }


        internal static void WriteJsArgs(StringBuilder sb)
        {
            sb.WriteJsValue("event.key");
            sb.Append('_');
            sb.WriteJsBoolean("event.altKey");
            sb.Append('_');
            sb.WriteJsBoolean("event.ctrlKey");
            sb.Append('_');
            sb.WriteJsBoolean("event.shiftKey");
            sb.Append('_');
            sb.WriteJsBoolean("event.metaKey");
            sb.Append('_');
            sb.WriteJsBoolean("event.repeat");
            sb.Append('_');
            sb.WriteJsValue("event.location");
            sb.Append('_');
            sb.WriteJsBoolean("event.isComposing");
        }

        internal static KeyEventArgs ReadJsArgs(ReadOnlySpan<char> args, ref MemoryExtensions.SpanSplitEnumerator<char> argRanges)
        {
            return new KeyEventArgs()
            {
                Key = args.ReadNextRawString(ref argRanges),
                AltKey = args.ReadNextBoolean(ref argRanges),
                CtrlKey = args.ReadNextBoolean(ref argRanges),
                ShiftKey = args.ReadNextBoolean(ref argRanges),
                MetaKey = args.ReadNextBoolean(ref argRanges),
                Repeat = args.ReadNextBoolean(ref argRanges),
                Location = args.ReadNextInt(ref argRanges),
                IsComposing = args.ReadNextBoolean(ref argRanges)
            };
        }
    }

    public class PointerEventArgs : EventArgs
    {
        public int Button { get; set; }
        public bool AltKey { get; set; }
        public bool CtrlKey { get; set; }
        public bool ShiftKey { get; set; }
        public bool MetaKey { get; set; }
        public double ClientX { get; set; }
        public double ClientY { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }

        internal static void WriteJsArgs(StringBuilder sb)
        {
            sb.WriteJsValue("event.button");
            sb.Append('_');
            sb.WriteJsBoolean("event.altKey");
            sb.Append('_');
            sb.WriteJsBoolean("event.ctrlKey");
            sb.Append('_');
            sb.WriteJsBoolean("event.shiftKey");
            sb.Append('_');
            sb.WriteJsBoolean("event.metaKey");
            sb.Append('_');
            sb.WriteJsValue("event.clientX");
            sb.Append('_');
            sb.WriteJsValue("event.clientY");
            sb.Append('_');
            sb.WriteJsValue("event.offsetX");
            sb.Append('_');
            sb.WriteJsValue("event.offsetY");
        }

        internal static PointerEventArgs ReadJsArgs(ReadOnlySpan<char> args, ref MemoryExtensions.SpanSplitEnumerator<char> argRanges)
        {
            return new PointerEventArgs()
            {
                Button = args.ReadNextInt(ref argRanges),
                AltKey = args.ReadNextBoolean(ref argRanges),
                CtrlKey = args.ReadNextBoolean(ref argRanges),
                ShiftKey = args.ReadNextBoolean(ref argRanges),
                MetaKey = args.ReadNextBoolean(ref argRanges),
                ClientX = args.ReadNextDouble(ref argRanges),
                ClientY = args.ReadNextDouble(ref argRanges),
                OffsetX = args.ReadNextDouble(ref argRanges),
                OffsetY = args.ReadNextDouble(ref argRanges),
            };
        }
    }

    public class InputEventArgs
    {
        public string Value { get; set; }
        public int SelectionStart{ get; set; }
        public int SelectionEnd { get; set; }

        internal static void WriteJsArgs(StringBuilder sb)
        {
            sb.WriteJsString("element.value");
            sb.Append('_');
            sb.WriteJsValue("element.selectionStart");
            sb.Append('_');
            sb.WriteJsValue("element.selectionEnd");
        }

        internal static InputEventArgs ReadJsArgs(ReadOnlySpan<char> args, ref MemoryExtensions.SpanSplitEnumerator<char> argRanges)
        {
            return new InputEventArgs()
            {
                Value = args.ReadNextString(ref argRanges),
                SelectionStart = args.ReadNextInt(ref argRanges),
                SelectionEnd = args.ReadNextInt(ref argRanges)
            };
        }
    }

    public enum WheelDeltaMode
    {
        Pixel = 0,
        Line = 1,
        Page = 2
    }

    public class WheelEventArgs : EventArgs
    {
        public double DeltaX { get; set; }
        public double DeltaY { get; set; }
        public WheelDeltaMode DeltaMode { get; set; }
        public bool AltKey { get; set; }
        public bool CtrlKey { get; set; }
        public bool ShiftKey { get; set; }
        public bool MetaKey { get; set; }
        public double ClientX { get; set; }
        public double ClientY { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }

        internal static void WriteJsArgs(StringBuilder sb)
        {
            sb.WriteJsValue("event.deltaX");
            sb.Append('_');
            sb.WriteJsValue("event.deltaY");
            sb.Append('_');
            sb.WriteJsValue("event.deltaMode");
            sb.Append('_');
            sb.WriteJsBoolean("event.altKey");
            sb.Append('_');
            sb.WriteJsBoolean("event.ctrlKey");
            sb.Append('_');
            sb.WriteJsBoolean("event.shiftKey");
            sb.Append('_');
            sb.WriteJsBoolean("event.metaKey");
            sb.Append('_');
            sb.WriteJsValue("event.clientX");
            sb.Append('_');
            sb.WriteJsValue("event.clientY");
            sb.Append('_');
            sb.WriteJsValue("event.offsetX");
            sb.Append('_');
            sb.WriteJsValue("event.offsetY");
        }

        internal static WheelEventArgs ReadJsArgs(ReadOnlySpan<char> args, ref MemoryExtensions.SpanSplitEnumerator<char> argRanges)
        {
            return new WheelEventArgs()
            {
                DeltaX = args.ReadNextDouble(ref argRanges),
                DeltaY = args.ReadNextDouble(ref argRanges),
                DeltaMode = (WheelDeltaMode)args.ReadNextInt(ref argRanges),
                AltKey = args.ReadNextBoolean(ref argRanges),
                CtrlKey = args.ReadNextBoolean(ref argRanges),
                ShiftKey = args.ReadNextBoolean(ref argRanges),
                MetaKey = args.ReadNextBoolean(ref argRanges),
                ClientX = args.ReadNextDouble(ref argRanges),
                ClientY = args.ReadNextDouble(ref argRanges),
                OffsetX = args.ReadNextDouble(ref argRanges),
                OffsetY = args.ReadNextDouble(ref argRanges),
            };
        }
    }

    public class SelectionChangeEventArgs
    {
        public int SelectionStart { get; set; }
        public int SelectionEnd { get; set; }

        internal static void WriteJsArgs(StringBuilder sb)
        {
            sb.WriteJsValue("element.selectionStart");
            sb.Append('_');
            sb.WriteJsValue("element.selectionEnd");
        }

        internal static SelectionChangeEventArgs ReadJsArgs(ReadOnlySpan<char> args, ref MemoryExtensions.SpanSplitEnumerator<char> argRanges)
        {
            return new SelectionChangeEventArgs()
            {
                SelectionStart = args.ReadNextInt(ref argRanges),
                SelectionEnd = args.ReadNextInt(ref argRanges)
            };
        }
    }

    public static class JsEventExtensions
    {
        internal static string ToHtmlEventName(this JsCommonEvent jsEvent)
        {
            switch (jsEvent)
            {
                case JsCommonEvent.Click: return "onclick";
                case JsCommonEvent.Dblclick: return "ondblclick";
                case JsCommonEvent.Pointerdown: return "onpointerdown";
                case JsCommonEvent.Pointermove: return "onpointermove";
                case JsCommonEvent.Pointerup: return "onpointerup";
                case JsCommonEvent.Pointerenter: return "onpointerenter";
                case JsCommonEvent.Pointerleave: return "onpointerleave";
                case JsCommonEvent.Pointerover: return "onpointerover";
                case JsCommonEvent.Pointerout: return "onpointerout";
                case JsCommonEvent.Pointercancel: return "onpointercancel";
                case JsCommonEvent.Keydown: return "onkeydown";
                case JsCommonEvent.Keyup: return "onkeyup";
                case JsCommonEvent.Input: return "oninput";
                case JsCommonEvent.Change: return "onchange";
                case JsCommonEvent.Focus: return "onfocus";
                case JsCommonEvent.Blur: return "onblur";
                case JsCommonEvent.Submit: return "onsubmit";
                case JsCommonEvent.Scroll: return "onscroll";
                case JsCommonEvent.Selectionchange: return "onselectionchange";
                case JsCommonEvent.Wheel: return "onwheel";
                default: return string.Empty;
            }
        }

        internal static bool ReadNextBoolean(this ReadOnlySpan<char> args, ref MemoryExtensions.SpanSplitEnumerator<char> argRanges)
        {
            var val = args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value);
            argRanges.MoveNext();
            return val != "0";
        }

        internal static int ReadNextInt(this ReadOnlySpan<char> args, ref MemoryExtensions.SpanSplitEnumerator<char> argRanges)
        {
            var val = int.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value));
            argRanges.MoveNext();
            return val;
        }

        internal static double ReadNextDouble(this ReadOnlySpan<char> args, ref MemoryExtensions.SpanSplitEnumerator<char> argRanges)
        {
            //JS numbers always use '.' decimals; must not parse with the machine culture
            var val = double.Parse(args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value),
                CultureInfo.InvariantCulture);
            argRanges.MoveNext();
            return val;
        }

        internal static string ReadNextRawString(this ReadOnlySpan<char> args, ref MemoryExtensions.SpanSplitEnumerator<char> argRanges)
        {
            var val = args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value);
            argRanges.MoveNext();
            return val.ToString();
        }

        internal static string ReadNextString(this ReadOnlySpan<char> args, ref MemoryExtensions.SpanSplitEnumerator<char> argRanges)
        {
            var val = args.Slice(argRanges.Current.Start.Value, argRanges.Current.End.Value - argRanges.Current.Start.Value);
            argRanges.MoveNext();
            var bytes = Convert.FromBase64String(val.ToString());
            return Encoding.UTF8.GetString(bytes);
        }

        internal static void WriteJsBoolean(this StringBuilder sb, string val)
        {
            sb.Append("${(").Append(val).Append(" ? '1' : '0')}");
        }

        internal static void WriteJsValue(this StringBuilder sb, string val)
        {
            sb.Append("${(").Append(val).Append(" ?? '0')}");
        }

        internal static void WriteJsString(this StringBuilder sb, string val)
        {
            sb.Append("${toBase64(").Append(val).Append(")}");
        }
    }
}
