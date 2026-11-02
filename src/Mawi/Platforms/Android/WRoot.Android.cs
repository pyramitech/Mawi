using Android.Webkit;
using Java.Interop;

namespace Mawi
{
    // All the code in this file is only included on Android.
    public partial class WRoot
    {
        private partial Task Initialize()
        {
            if (this?.Handler?.PlatformView is Android.Webkit.WebView view)
            {
                view.Settings.JavaScriptEnabled = true;
                view.AddJavascriptInterface(new WRootJavaScriptInterface(this), "wRootHost");

                view.LongClickable = false;
                view.HapticFeedbackEnabled = false;
                view.SetOnLongClickListener(new ConsumeLongClick());
                //Initialized = true;
            }

            return Task.CompletedTask;
        }

        class ConsumeLongClick : Java.Lang.Object, Android.Views.View.IOnLongClickListener
        {
            public bool OnLongClick(Android.Views.View v) => true; //consume
        }

        private sealed class WRootJavaScriptInterface : Java.Lang.Object
        {
            private readonly WRoot _root;

            public WRootJavaScriptInterface(WRoot root)
            {
                _root = root;
            }

            [JavascriptInterface]
            [Export("sendMessage")]
            public void SendMessage(string message)
            {
                _root.ProcessMessage(message);
            }
        }
    }
}
