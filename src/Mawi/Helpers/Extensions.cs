using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Helpers
{
    internal static class DoubleExtensions
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        internal static string ToInv(this double v) => v.ToString(Inv);
    }

    internal static class StringExtensions
    {
        private static readonly SearchValues<char> _unsafeChars = SearchValues.Create("<>\"\\'\n\r");
        internal static string MakeHtmlSafe(this string text)
        {
            var span = text.AsSpan();
            int index = span.IndexOfAny(_unsafeChars);
            if (index < 0) return text;

            var sb = new StringBuilder(text.Length + 8);
            while (index >= 0)
            {
                sb.Append(span.Slice(0, index));
                switch (span[index])
                {
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    case '"': sb.Append("&quot;"); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("<br/>"); break;
                    case '\r': break; //discard
                    case '\'': sb.Append("\\'"); break;
                }
                span = span.Slice(index + 1);
                index = span.IndexOfAny(_unsafeChars);
            }
            sb.Append(span);
            return sb.ToString();
        }

        private static readonly SearchValues<char> _unsafeAttrChars = SearchValues.Create("<>\"&\\\n\r");

        /// <summary>
        /// makes a value safe to embed inside a \"..\" delimited html attribute (or inline style)
        /// that itself lives inside the js double-quoted string passed to Eval
        /// </summary>
        internal static string MakeAttributeSafe(this string text)
        {
            var span = text.AsSpan();
            int index = span.IndexOfAny(_unsafeAttrChars);
            if (index < 0) return text;

            var sb = new StringBuilder(text.Length + 8);
            while (index >= 0)
            {
                sb.Append(span.Slice(0, index));
                switch (span[index])
                {
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    case '"': sb.Append("&quot;"); break;
                    case '&': sb.Append("&amp;"); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': break; //discard
                }
                span = span.Slice(index + 1);
                index = span.IndexOfAny(_unsafeAttrChars);
            }
            sb.Append(span);
            return sb.ToString();
        }
    }
}
