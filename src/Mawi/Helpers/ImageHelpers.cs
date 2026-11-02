using Mawi.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi.Helpers
{
    internal static class ImageHelpers
    {
        internal static void LoadImage(IImageContainer sender, WRoot root, ImageSource source)
        {
            if (source is FileImageSource imgSource)
            {
                var filePath = imgSource.File;
                var res = root.LoadImage(filePath, sender);

                sender.SourceUrl = res;                
            }
            else if (source is UriImageSource uriImageSource)
            {
                sender.SourceUrl = uriImageSource.Uri.ToString();
            }
            else if (source is StreamImageSource streamImageSource)
            {
                streamImageSource.Stream.Invoke(new CancellationToken()).ContinueWith(x =>
                {
                    if (x.IsCompletedSuccessfully)
                    {
                        var stream = x.Result;

                        using (var memstream = new MemoryStream())
                        {
                            stream.CopyTo(memstream);

                            //generate local web url here
                            var res = root.LoadImage(memstream.ToArray(), sender);

                            if (!string.IsNullOrEmpty(res))
                            {
                                sender.SourceUrl = res;
                            }
                        }
                    }
                });
            }
        }
    }
}
