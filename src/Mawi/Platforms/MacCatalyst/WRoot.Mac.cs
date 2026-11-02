using Foundation;
using WebKit;

namespace Mawi
{
    // All the code in this file is only included on Mac Catalyst.
    public partial class WRoot
    {
        const string HandlerName = "mawi";
        ScriptHandler _scriptHandler;

        private partial Task Initialize()
        {
            if (this?.Handler?.PlatformView is WKWebView view)
            {
                view.Configuration.UserContentController.RemoveScriptMessageHandler(HandlerName);

                _scriptHandler?.Dispose();
                _scriptHandler = new ScriptHandler(this);

                view.Configuration.UserContentController.AddScriptMessageHandler(_scriptHandler, HandlerName);
                if (this.Settings != null && this.Settings.EnableDevTools)
                {
                    if (OperatingSystem.IsMacCatalystVersionAtLeast(major: 13, minor: 3) ||
                    OperatingSystem.IsIOSVersionAtLeast(major: 16, minor: 4))
                    {
#pragma warning disable CA1416 // Validate platform compatibility
                        view.Inspectable = true;
#pragma warning restore CA1416 // Validate platform compatibility
                    }
                }
            }

            return Task.CompletedTask;
        }

        private sealed class ScriptHandler : NSObject, IWKScriptMessageHandler
        {
            readonly WeakReference<WRoot> _rootRef;

            public ScriptHandler(WRoot root) => _rootRef = new WeakReference<WRoot>(root);

            [Export("userContentController:didReceiveScriptMessage:")]
            public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
            {
                if (!_rootRef.TryGetTarget(out var root))
                    return;

                var payload = message.Body?.ToString() ?? null;
                if (payload != null)
                    root.ProcessMessage(payload);
            }
        }
    }
}
