using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi
{
    public class WFilterList
    {
        private WFilter[] values;

        public int Length => values.Length;

        public WFilterList(params WFilter[] args)
        {
            values = args;
        }

        public void Write(StringBuilder sb)
        {
            values[0].Write(sb);
            for (int i = 1; i < values.Length; i++)
            {
                sb.Append(' ');
                values[i].Write(sb);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(128);
            Write(sb);
            return sb.ToString();
        }
    }

    public class WShadowList
    {
        private WShadow[] values;

        public int Length => values.Length; 

        public WShadowList(params WShadow[] args)
        {
            values = args;
        }

        public void Write(StringBuilder sb)
        {
            values[0].Write(sb);
            for (int i = 1; i < values.Length; i++)
            {
                sb.Append(',');
                values[i].Write(sb);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(128);
            Write(sb);
            return sb.ToString();
        }
    }
}
