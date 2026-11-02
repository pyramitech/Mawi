using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi
{
    public class WCachedImageSource : ImageSource
    {
        internal string SourceUrl { get; private set; }
        internal WRoot Root { get; private set; }

        private WCachedImageSource() { }

        ~WCachedImageSource()
        {

        }

        internal static WCachedImageSource CreateCache(WRoot root, string sourceUrl)
        {
            return new WCachedImageSource() { SourceUrl = sourceUrl, Root = root };
        }
    }
}
