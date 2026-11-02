namespace Mawi
{
    public partial class WRoot
    {
        private partial async Task Initialize()
        {
            var view = (this.Handler.PlatformView as Microsoft.UI.Xaml.Controls.WebView2);

            view.CoreWebView2Initialized += async (s, e) =>
            {
                var core = view.CoreWebView2;
                var settings = core.Settings;

                if (this.Settings == null || !this.Settings.EnableDevTools)
                {
                    settings.AreDevToolsEnabled = false;
                    settings.AreDefaultContextMenusEnabled = false;
                }
                if (this.Settings != null && this.Settings.EmulateTouch)
                {
                    await core.CallDevToolsProtocolMethodAsync(
                        "Emulation.setTouchEmulationEnabled",
                        """{ "enabled": true, "maxTouchPoints": 5 }"""
                    );
                }
                settings.AreBrowserAcceleratorKeysEnabled = false;
            };

            await view.EnsureCoreWebView2Async();

            view.CoreWebView2.Settings.IsWebMessageEnabled = true;
            view.WebMessageReceived += WRoot_WebMessageReceived;
            //Initialized = true;
        }

        private void WRoot_WebMessageReceived(Microsoft.UI.Xaml.Controls.WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs args)
        {
            var span = args.WebMessageAsJson.AsSpan(1, args.WebMessageAsJson.Length - 2);
            ProcessMessage(span);
        }
    }
}
