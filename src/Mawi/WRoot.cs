using Mawi.Animation;
using Mawi.Helpers;
using Mawi.Views;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Hashing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Mawi
{
    [ContentProperty(nameof(Content))]
    public partial class WRoot : WebView, IWRenderTarget
    {
        private static readonly HashSet<string> EmbeddedResources = new(
            Assembly.GetExecutingAssembly().GetManifestResourceNames(),
            StringComparer.OrdinalIgnoreCase
        );

        private List<WView> UpdateList = new(512);
        private List<WView> RemoveList = new(512);
        private List<(WView, WAnimation, Guid, TaskCompletionSource)> AnimateList = new(128);
        private List<WView> CancelAnimateList = new(128);
        internal List<WView> ObserveList = new(128);
        internal List<WView> UnobserveList = new(128);
        internal ConcurrentDictionary<long, WElement> Views = new();
        internal ConcurrentDictionary<string, string> Images = new();
        internal ConcurrentDictionary<Guid, TaskCompletionSource> AnimationCompletion = new();
        private List<string> _commands = new(128);
        public static Action<string> RenderDebugAction { get; set; }
        public static Action<Exception> ErrorDebugAction { get; set; }

        internal volatile bool Initialized = false;

        /// <summary>
        /// Default style has to be set before WRoot initializes, it is written directly into the default style tag
        /// </summary>
        public static string DefaultStyle = string.Empty;

        private StringBuilder _builder = new(10 * 1024 * 1024); //10MB initial
        private WView _content;

        private static string _baseHtml;

        private static string GetBaseHtml()
        {
            if (_baseHtml == null)
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Mawi.base.html");
                using var reader = new StreamReader(stream, Encoding.UTF8);
                _baseHtml = reader.ReadToEnd();
            }
            return _baseHtml;
        }

        internal SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private int _queued = 0;

        internal void QueueRender()
        {
            //before initialization work only accumulates, the init probe queues the first flush
            if (!Initialized)
                return;

            if (Interlocked.CompareExchange(ref _queued, 1, 0) != 0)
                return;

            try
            {
                this.Dispatcher.Dispatch(() =>
                {
                    Volatile.Write(ref _queued, 0);
                    this.Render();
                });
            }
            catch (Exception ex)
            {
                Volatile.Write(ref _queued, 0);
                if (ErrorDebugAction != null)
                {
                    ErrorDebugAction(ex);
                }
            }
        }

        public WView Content
        {
            get => _content;
            set
            {
#if DEBUG
                if (!MainThread.IsMainThread)
                {
                    ErrorDebugAction?.Invoke(new InvalidOperationException(
                        "WRoot.Content set off the main thread, marshal the call with MainThread.BeginInvokeOnMainThread"));
                }
#endif
                if (_content != null)
                {
                    _content.UpdateState = UpdateState.Removed;
                    this.RemoveView(_content);
                }
                _content = value;
                if (value != null)
                {
                    _content.Parent = this;
                    if (BindingContext != null)
                    {
                        SetInheritedBindingContext(_content, BindingContext);
                    }
                    this.UpdateView(_content);
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

        public WRoot()
        {
            
        }

        /// <summary>
        /// This has to be set BEFORE handler is set
        /// </summary>
        public RootSettings Settings { get; set; }

        protected override void OnHandlerChanged()
        {
            if (this.Handler == null)
            {
                //webview is being torn down, don't leak it and don't let renders eval into a dead handler
                Initialized = false;
                WRootRuntime.UnregisterWebView(this);
                base.OnHandlerChanged();
                return;
            }

            WRootRuntime.RegisterWebView(this);
            Initialize();

            var obj = GetBaseHtml();
            var vals = Enum.GetValues(typeof(JsCommonEvent));
            var sb = new StringBuilder(1024);
            foreach (JsCommonEvent v in vals)
            {
                if (v == JsCommonEvent.Pointerdown || v == JsCommonEvent.Scroll || v == JsCommonEvent.Resize)
                    continue; //skip pointerdown, scroll and resize, these have a custom implementation

                sb.Append("function _").Append(v.ToHtmlEventName()).Append("(event){ ");
                if (v == JsCommonEvent.Wheel)
                {
                    //ctrl+wheel (and trackpad pinch, which chromium delivers as ctrl+wheel) would
                    //zoom the page before the app ever sees the event. element-level onwheel
                    //handlers are non-passive, so preventDefault here actually works
                    sb.Append("if(event.ctrlKey){event.preventDefault();} ");
                }
                sb.Append("event.stopPropagation(); const element = event.currentTarget; const rect = element.getBoundingClientRect(); sendMsg(`${element.id.slice(1)}_e_").Append((int)v).Append("_${rect.x}_${rect.y}_${rect.width}_${rect.height}_${element.scrollLeft}_${element.scrollTop}_${element.scrollWidth}_${element.scrollHeight}");
                switch (v)
                {
                    case JsCommonEvent.Keydown:
                    case JsCommonEvent.Keyup:
                        sb.Append('_');
                        KeyEventArgs.WriteJsArgs(sb);
                        break;

                    case JsCommonEvent.Click:
                    case JsCommonEvent.Dblclick:
                    case JsCommonEvent.Pointerdown:
                    case JsCommonEvent.Pointermove:
                    case JsCommonEvent.Pointerup:
                    case JsCommonEvent.Pointerenter:
                    case JsCommonEvent.Pointerleave:
                    case JsCommonEvent.Pointerover:
                    case JsCommonEvent.Pointerout:
                    case JsCommonEvent.Pointercancel:
                        sb.Append('_');
                        PointerEventArgs.WriteJsArgs(sb);
                        break;

                    case JsCommonEvent.Input:
                    case JsCommonEvent.Change:
                        sb.Append('_');
                        InputEventArgs.WriteJsArgs(sb);
                        break;

                    case JsCommonEvent.Selectionchange:
                        sb.Append('_');
                        SelectionChangeEventArgs.WriteJsArgs(sb);
                        break;

                    case JsCommonEvent.Wheel:
                        sb.Append('_');
                        WheelEventArgs.WriteJsArgs(sb);
                        break;
                }
                sb.AppendLine("`);}");
            }

            obj = obj.Replace("{eventscripts}", sb.ToString());

            sb.Clear();

            int i = 0;
            foreach (JsCommonEvent v in vals)
            {
                if (v == JsCommonEvent.Resize)
                    continue;

                sb.Append("element.").Append(v.ToHtmlEventName()).Append(" = content[")
                    .Append(i).Append("] === '0' ? null : _").Append(v.ToHtmlEventName()).AppendLine(";");
                ++i;
            }

            obj = obj.Replace("{eventupdates}", sb.ToString());

            obj = obj.Replace("{defaultStyle}", WRoot.DefaultStyle);

            if (Settings != null && Settings.OverrideBodyLineHeight.HasValue)
            {
                obj = obj.Replace("{bodystyle}", "line-height: 1.2;");
            }
            else
            {
                obj = obj.Replace("{bodystyle}", "");
            }

            this.Source = new HtmlWebViewSource
            {
                Html = obj
            };
            Task.Run(async () =>
            {
                while (true)
                {
                    if (this.Handler == null) //handler died while we were still probing
                        return;

                    try
                    {
                        var res = await MainThread.InvokeOnMainThreadAsync<bool>(async () =>
                        {
                            try
                            {
                                //a successful eval of the sentinel proves the eval channel works end to end
                                //AND that the loaded document is ours (about:blank reports readyState "complete" too,
                                //but has no __mawiReady) AND that the whole head script executed
                                var state = await this.EvaluateJavaScriptAsync("window.__mawiReady === 1 ? 1 : 0");
                                return state is string s && s == "1";
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e);
                            }
                            return false;
                        });
                        if (res)
                        {
                            this.Initialized = true;
                            this.QueueRender(); //flush whatever accumulated while the page was loading
                            return;
                        }
                    }
                    catch (Exception ex) //throws if can't find main thread..?
                    {
                        Debug.WriteLine(ex);
                    }
                    await Task.Delay(100);
                }
            });

            base.OnHandlerChanged();
        }

        /// <summary>
        /// loads the script into global scope
        /// </summary>
        /// <param name="script"></param>
        public void LoadScript(string script)
        {
            var data = Convert.ToBase64String(Encoding.UTF8.GetBytes(script));
            Execute($"loadScript('{data}');");
        }

        /// <summary>
        /// loads ESM script into window as `window[moduleName]`
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="script"></param>
        public void LoadScriptModule(string moduleName, string script)
        {
            var data = Convert.ToBase64String(Encoding.UTF8.GetBytes(script));
            Execute($"loadScriptModule('{moduleName}','{data}');");
        }

        /// <summary>
        /// loads the style by creating a new style tag with specified id
        /// </summary>
        public void LoadStyle(string id, string cssText)
        {
            var data = Convert.ToBase64String(Encoding.UTF8.GetBytes(cssText));
            Execute($"loadStyle('{id}','{data}');");
        }

        /// <summary>
        /// deletes the style tag with the specified id
        /// </summary>
        public void UnloadStyle(string id)
        {
            Execute($"unloadStyle('{id}');");
        }

        /// <summary>
        /// applies the scrollbar theme to every scrollable element in the document,
        /// null removes it. per-view themes: theme.ToCss(".my-class") + LoadStyle + StyleClasses
        /// </summary>
        public void SetScrollbarTheme(WScrollbarTheme theme)
        {
            const string styleId = "mawi-scrollbar-theme";
            if (theme == null)
            {
                UnloadStyle(styleId);
            }
            else
            {
                LoadStyle(styleId, theme.ToCss("*"));
            }
        }

        /// <summary>
        /// executes javascript
        /// </summary>
        /// <param name="script"></param>
        internal void Execute(string script)
        {
            lock (this)
            {
                _commands.Add(script);
            }
            this.QueueRender();
        }

        internal async void UnloadImage(string uri)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (Images.TryRemove(uri, out var image))
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        this.EvaluateJavaScriptAsync("unloadImage('" + image + "');");
                    });
                }
            }
            catch (Exception ex)
            {
                if (ErrorDebugAction != null)
                {
                    ErrorDebugAction(ex);
                }
            }
            finally
            {
                _semaphore.Release();
            }

        }

        internal string LoadImage(string uri, IImageContainer view)
        {
            if (string.IsNullOrEmpty(uri))
                return null;

            if (Images.TryGetValue(uri, out var res))
            {
                return res;
            }

            Task.Run(async () =>
            {
                await _semaphore.WaitAsync();
                try
                {
                    if (Images.TryGetValue(uri, out var res))
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            view.SourceUrl = res;
                            (view as WView).Invalidate();
                        });
                        return;
                    }

                    byte[] imgData = null;
#if ANDROID
                    try
                    {
                        using var stream = await FileSystem.OpenAppPackageFileAsync(uri);
                        using var ms = new MemoryStream();
                        await stream.CopyToAsync(ms);
                        imgData = ms.ToArray();
                    }
                    catch
                    {
                        return;
                    }
#else
                    if (File.Exists(uri))
                    {
                        imgData = File.ReadAllBytes(uri);
                    }
                    else
                    {
                        return;
                        //await using var stream = await FileSystem.OpenAppPackageFileAsync(uri);

                        //using var ms = new MemoryStream();
                        //await stream.CopyToAsync(ms);
                        //imgData = ms.ToArray();
                    }
#endif
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        var data = Convert.ToBase64String(imgData);
                        var url = await this.EvaluateJavaScriptAsync("loadImage('" + data + "');");
                        Images[uri] = url;

                        view.SourceUrl = url;
                        (view as WView).Invalidate();
                    });
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    _semaphore.Release();
                }
            });

            return null;            
        }

        internal string LoadImage(byte[] data, IImageContainer view)
        {
            var hc = new HashCode();
            hc.AddBytes(data);
            var hash = hc.ToHashCode().ToString();

            if (Images.TryGetValue(hash, out var res))
            {
                return res;
            }

            Task.Run(async () =>
            {
                await _semaphore.WaitAsync();
                try
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        var url = "";
                        if (Images.TryGetValue(hash, out var res))
                        {
                            url = res;
                        }
                        else
                        {
                            var dataBase = Convert.ToBase64String(data);
                            url = await this.EvaluateJavaScriptAsync("loadImage('" + dataBase + "');");
                            Images[hash] = url;
                        }

                        view.SourceUrl = url;
                    });
                }
                finally
                {
                    _semaphore.Release();
                }
            });

            return null;
        }

        /// <summary>
        /// loads and caches the image from a path so there's no delay at application runtime
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name">if provided it will be loaded and has to be used as the name instead of the path.</param>
        /// <returns>if loaded correctly true, otherwise if it can't find the file or the name is taken then it will return false</returns>
        public async ValueTask<bool> LoadImageFromFile(string path, string name = null)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (string.IsNullOrEmpty(name))
                    name = path;

                byte[] imgData = null;
                if (File.Exists(path))
                {
                    imgData = File.ReadAllBytes(path);
                }
                else
                {
                    return false;
                }

                if (Images.TryGetValue(name, out var oldUrl))
                {
                    await this.EvaluateJavaScriptAsync($"unloadImage('{oldUrl}');");
                }

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var data = Convert.ToBase64String(imgData);
                    var url = await this.EvaluateJavaScriptAsync("loadImage('" + data + "');");
                    Images[name] = url;
                });
                return true;
            }
            finally
            {
                _semaphore.Release();
            }

        }

        /// <summary>
        /// loads image into memory and sets it's name
        /// the loaded image can be used by setting source as the provided name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="imgData"></param>
        /// <returns></returns>
        public async ValueTask<bool> LoadImageFromData(string name, byte[] imgData)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (string.IsNullOrEmpty(name))
                    return false;

                if (Images.TryGetValue(name, out var oldUrl))
                {
                    await this.EvaluateJavaScriptAsync($"unloadImage('{oldUrl}');");
                }

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var data = Convert.ToBase64String(imgData);
                    var url = await this.EvaluateJavaScriptAsync("loadImage('" + data + "');");
                    Images[name] = url;
                });
                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// loads image into memory from embedded resources
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async ValueTask<bool> LoadImageFromResources(string name)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (string.IsNullOrEmpty(name))
                    return false;

                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"{assembly.GetName().Name}.{name.Replace('/', '.').Replace('\\', '.')}";
                if(!EmbeddedResources.Contains(resourceName))
                {
                    return false;
                }

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream is null) return false;

                var imgData = new byte[stream.Length];
                await stream.ReadExactlyAsync(imgData);

                if (Images.TryGetValue(name, out var oldUrl))
                {
                    await this.EvaluateJavaScriptAsync($"unloadImage('{oldUrl}');");
                }

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var data = Convert.ToBase64String(imgData);
                    var url = await this.EvaluateJavaScriptAsync("loadImage('" + data + "');");
                    Images[name] = url;
                });
                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public bool ImageCacheExists(string key) => Images.ContainsKey(key);

        internal void Render()
        {
            lock (this)
            {
                if (_content != null)
                {
                    if (_content.Root == null)
                    {
                        _content.AssignRoot(this);
                    }

                    if ((_content.UpdateState & UpdateState.Dirty) == UpdateState.Dirty) //if full dirty completely re-render
                    {
                        _content.UpdateState = UpdateState.None;
                        _builder.Append("updateBody(\"");
                        _content.Render(_builder);
                        _builder.Append("\");");

                        //base.Eval(string.Format("updateBody(\"{0}\");", _builder.ToString()));
#if DEBUG
                        var fin = _builder.ToString();
                        Debug.WriteLine(fin);
#endif
                        if (RenderDebugAction != null)
                            RenderDebugAction(_builder.ToString());

                        try
                        {
                            base.Eval(_builder.ToString());
                        }
                        catch (Exception ex)
                        {
                            //the tree is the source of truth, recover by re-rendering everything
                            //(the QueueRender below retries)
                            _content.UpdateState = UpdateState.Dirty;
                            if (ErrorDebugAction != null)
                            {
                                ErrorDebugAction(ex);
                            }
                        }
                        this.UpdateList.Clear();
                        this.RemoveList.Clear();
                        this.AnimateList.Clear();
                        this.CancelAnimateList.Clear();
                        _builder.Clear();

                        //enqueue another render in case there's more commands remaining to execute
                        this.QueueRender();
                    }
                    else if (UpdateList.Count != 0
                        || RemoveList.Count != 0
                        || AnimateList.Count != 0
                        || CancelAnimateList.Count != 0
                        || ObserveList.Count != 0
                        || UnobserveList.Count != 0
                        || _commands.Count != 0)
                    {
                        for (int i = 0; i < UpdateList.Count; i++)
                        {
                            WView update = UpdateList[i];
                            if ((update.UpdateState & UpdateState.Dirty) == UpdateState.Dirty)
                            {
                                _builder.Append("updateView(\"w").Append(update.Id).Append("\",\"");
                                update.Render(_builder);
                                _builder.Append("\");");
                            }
                            else
                            {
                                if ((update.UpdateState & UpdateState.DirtyAdd) != 0)
                                {
                                    if (update is WContainer container)
                                    {
                                        _builder.Append("addViews(\"w").Append(update.Id).Append("\",\"");
                                        foreach (var item in container.Children)
                                        {
                                            if (item.UpdateState == UpdateState.New)
                                            {
                                                item.Render(_builder);
                                            }
                                        }
                                        _builder.Append("\");");
                                    }
                                    else if (update is ICustomContainer customContainer)
                                    {
                                        if (update is IPrependContainer prependContainer && prependContainer.HasPrependViews)
                                        {
                                            _builder.Append("prependViews(\"w").Append(update.Id).Append("\",\"");
                                            prependContainer.RenderPrependViews(_builder);
                                            _builder.Append("\");");
                                        }
                                        _builder.Append("addViews(\"w").Append(update.Id).Append("\",\"");
                                        customContainer.RenderNewViews(_builder);
                                        _builder.Append("\");");
                                    }
                                }
                                if ((update.UpdateState & UpdateState.DirtyValue) != 0 && update is WEntryBase input)
                                {
                                    _builder.Append("setValue(\"w").Append(update.Id).Append("\",\"").Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(input.Text ?? string.Empty)));
                                    _builder.Append("\");");
                                }
                                if ((update.UpdateState & UpdateState.DirtyStyle) != 0)
                                {
                                    var container = update as WContainer;
                                    _builder.Append("updateViewStyle(\"w").Append(update.Id).Append("\",\"");
                                    update.StyleClasses?.ForEach(x => _builder.Append(x.MakeAttributeSafe()).Append(' '));
                                    _builder.Append("\",\"");
                                    update.RenderStyle(_builder);
                                    _builder.Append("\");");
                                }

                                //need manual reset
                                update.UpdateState = UpdateState.None;
                            }
                        }

                        if (RemoveList.Count != 0)
                        {
                            _builder.Append("removeViews(\"");
                            for (int i = 0; i < RemoveList.Count; i++)
                            {
                                var item = RemoveList[i];
                                if (i != 0)
                                {
                                    _builder.Append(",");
                                }
                                _builder.Append("#w").Append(item.Id);
                                item.UnassignRoot();
                            }
                            _builder.Append("\");");
                        }

                        if (AnimateList.Count != 0)
                        {
                            for (int i = 0; i < AnimateList.Count; i++)
                            {
                                var item = AnimateList[i];
                                if (item.Item4 == null)
                                {
                                    _builder.Append("animate('w")
                                    .Append(item.Item1.Id)
                                    .Append("',");
                                    item.Item2.Write(_builder);
                                    _builder.Append(");");
                                }
                                else
                                {
                                    _builder.Append("animateAsync('w")
                                    .Append(item.Item1.Id)
                                    .Append("','").Append(item.Item3).Append("',");
                                    item.Item2.Write(_builder);
                                    _builder.Append(");");
                                }
                            }
                        }

                        if (CancelAnimateList.Count != 0)
                        {
                            for (int i = 0; i < CancelAnimateList.Count; i++)
                            {
                                var item = CancelAnimateList[i];
                                _builder.Append("cancelAnimations('w")
                                .Append(item.Id);
                                _builder.Append("');");
                            }
                        }

                        if (ObserveList.Count != 0)
                        {
                            for (int i = 0; i < ObserveList.Count; i++)
                            {
                                var item = ObserveList[i];
                                _builder.Append("observeElement('w")
                                .Append(item.Id);
                                _builder.Append("');");
                            }
                        }

                        if (UnobserveList.Count != 0)
                        {
                            for (int i = 0; i < UnobserveList.Count; i++)
                            {
                                var item = UnobserveList[i];
                                _builder.Append("unobserveElement('w")
                                .Append(item.Id);
                                _builder.Append("');");
                            }
                        }

                        if (_commands.Count != 0)
                        {
                            _builder.Append("try {");
                            for (int i = 0; i < _commands.Count; i++)
                            {
                                _builder.Append(_commands[i]);
                            }
                            _builder.Append("} catch (e) { console.error(e); }");
                        }

#if DEBUG
                        var fin = _builder.ToString();
                        Debug.WriteLine(fin);
#endif
                        if (RenderDebugAction != null)
                            RenderDebugAction(_builder.ToString());

                        try
                        {
                            base.Eval(_builder.ToString());
                            _commands.Clear();
                        }
                        catch (Exception ex)
                        {
                            //commands can't be re-derived from the tree so they are kept for the retry,
                            //everything else is recovered by a full re-render
                            _content.UpdateState = UpdateState.Dirty;
                            if (ErrorDebugAction != null)
                            {
                                ErrorDebugAction(ex);
                            }
                            this.QueueRender();
                        }
                        this.UpdateList.Clear();
                        this.RemoveList.Clear();
                        this.AnimateList.Clear();
                        this.CancelAnimateList.Clear();
                        this.ObserveList.Clear();
                        this.UnobserveList.Clear();
                        _builder.Clear();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UpdateView(WView view)
        {
            lock (this)
            {
                this.UpdateList.Add(view);
            }
            this.QueueRender();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveView(WView view)
        {
            lock (this)
            {
                this.RemoveList.Add(view);
            }
            this.QueueRender();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AnimateView((WView, WAnimation, Guid, TaskCompletionSource) view)
        {
            lock (this)
            {
                this.AnimateList.Add(view);
            }
            this.QueueRender();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CancelAnimateView(WView view)
        {
            lock (this)
            {
                this.CancelAnimateList.Add(view);
            }
            this.QueueRender();
        }

        /// <summary>
        /// Add font to root
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <param name="filePath"></param>
        /// <param name="weight">can be range like "100 900"</param>
        /// <param name="style"></param>
        /// <returns></returns>
        public async ValueTask AddFont(string fontFamily, string filePath, string weight, FontStyleType style)
        {
            byte[] data;
            if (File.Exists(filePath))
            {
                data = await File.ReadAllBytesAsync(filePath);
            }
            else
            {
                return;
            }
            AddFont(fontFamily, data, Path.GetExtension(filePath), weight, style);
        }

        /// <summary>
        /// Add font to root
        /// </summary>
        /// <param name="fontFamily"></param>
        /// <param name="data">data of the font file</param>
        /// <param name="fileExtension">extension of the font for example ttf of woff2 etc</param>
        /// <param name="weight">can be range like "100 900"</param>
        /// <param name="style"></param>
        public void AddFont(string fontFamily, byte[] data, string fileExtension, string weight, FontStyleType style)
        {
            if (fileExtension.StartsWith('.'))
            {
                fileExtension = fileExtension.Substring(1);
            }
            var lowerExtension = fileExtension.ToLower();
            Execute($"addFont(\"{fontFamily}\", \"{Convert.ToBase64String(data)}\", \"{lowerExtension}\", \"{weight}\", \"{style.ToString().ToLower()}\");");
        }

        /// <summary>
        /// Add font URL to root (like google fonts)
        /// </summary>
        /// <param name="url"></param>
        public void AddFont(string url)
        {
            Execute($"addFontLink(\"{Convert.ToBase64String(Encoding.UTF8.GetBytes(url))}\");");
        }

        public void Render(StringBuilder sb)
        {
            throw new NotImplementedException();
        }

        public void RenderChildLayout(WView child, StringBuilder sb)
        {
            sb.Append("justify-self:");
            switch (child.HorizontalOptions.Alignment)
            {
                case LayoutAlignment.Fill:
                    if (child.WidthRequest == -1)
                    {
                        sb.Append("stretch");
                    }
                    else
                    {
                        sb.Append("center");
                    }
                    break;

                case LayoutAlignment.Start:
                    sb.Append("start");
                    break;

                case LayoutAlignment.Center:
                    sb.Append("center");
                    break;

                case LayoutAlignment.End:
                    sb.Append("end");
                    break;
            }
            sb.Append(";");

            sb.Append("align-self:");
            switch (child.VerticalOptions.Alignment)
            {
                case LayoutAlignment.Fill:
                    if (child.HeightRequest == -1)
                    {
                        sb.Append("stretch");
                    }
                    else
                    {
                        sb.Append("center");
                    }
                    break;

                case LayoutAlignment.Start:
                    sb.Append("start");
                    break;

                case LayoutAlignment.Center:
                    sb.Append("center");
                    break;

                case LayoutAlignment.End:
                    sb.Append("end");
                    break;
            }
            sb.Append(";");
        }

        private void ProcessMessage(ReadOnlySpan<char> msg)
        {
            var splitter = msg.Split('_');

            if (splitter.Current.End.Value == 0)
                splitter.MoveNext();

            var id = msg.Slice(0, splitter.Current.End.Value);
            splitter.MoveNext();

            var type = msg[splitter.Current.Start.Value..splitter.Current.End.Value];
            splitter.MoveNext();

            if (type.SequenceEqual("e"))
            {
                var ev = msg[splitter.Current.Start.Value..splitter.Current.End.Value];
                splitter.MoveNext();

                var idnum = long.Parse(id);
                var jsev = (JsCommonEvent)int.Parse(ev);

                if (Views.TryGetValue(idnum, out var view))
                {
                    view.InvokeEvent(jsev, msg, splitter);
                }
            }
            else if (type.SequenceEqual("r"))
            {
                var ev = msg[splitter.Current.Start.Value..splitter.Current.End.Value];
                splitter.MoveNext();

                var idnum = long.Parse(id);
                var jsev = int.Parse(ev); //TODO: enum, currently only 0 = measure

                if (Views.TryGetValue(idnum, out var view))
                {
                    if (jsev == 1) //return animation
                    {
                        var animGuid = msg[splitter.Current.Start.Value..splitter.Current.End.Value];
                        splitter.MoveNext();
                        if (AnimationCompletion.TryRemove(Guid.Parse(animGuid), out var value))
                        {
                            value.TrySetResult();
                        }
                    }

                    if (jsev == 2) //virtualized list children measure, custom payload
                    {
                        (view as WVirtualPanel)?.OnChildrenMeasured(msg, splitter);
                    }
                    else
                    {
                        view.InvokeReturn(jsev, msg, splitter);
                    }
                }
            }
        }

        //TEMPORARY --- REMOVE LATER
        public async Task<int> CheckBody()
        {
            var res = MainThread.IsMainThread
                ? await base.EvaluateJavaScriptAsync("checkBody()")
                : await MainThread.InvokeOnMainThreadAsync(() => base.EvaluateJavaScriptAsync("checkBody()"));

            return int.TryParse(res, out var val) ? val : -1;
        }

        private partial Task Initialize();
    }

    public class RootSettings
    {
        public bool EnableDevTools;
        public bool EmulateTouch;
        public decimal? OverrideBodyLineHeight;
    }

    public static class WRootRuntime 
    {
        private static long NextId = 1;
        internal static long RunId { get; private set; }

        private static List<WRoot> Roots = new();

        internal static long RequestId()
        {
            return NextId++;
        }

        internal static void RegisterWebView(WRoot root)
        {
            lock (Roots)
            {
                if (!Roots.Contains(root))
                {
                    Roots.Add(root);
                }
            }
        }

        internal static void UnregisterWebView(WRoot root)
        {
            lock (Roots)
            {
                Roots.Remove(root);
            }
        }

        //internal static void Render()
        //{
        //    ++RunId;
        //    foreach (var root in Roots)
        //    {
        //        if (root.Initialized)
        //        {
        //            root.Render();
        //        }
        //    }
        //}

//        public static void Start()
//        {
//            Task.Run(async () => {
//                await Task.Delay(1000);
//                while (true)
//                {
//                    try
//                    {
//                        await Task.Delay(5);
//                        await MainThread.InvokeOnMainThreadAsync(() => { Render(); });
//                    }
//                    catch (Exception ex)
//                    {
//#if DEBUG
//                        Debug.WriteLine(ex);
//#endif
//                    }
//                }
//            });
//        }
    }

    public static class Extensions
    {

        public static ulong GetXxHash64(ReadOnlySpan<byte> data, int seed = 0)
        {
            return BitConverter.ToUInt64(XxHash3.Hash(data, seed), 0);
        }
    }
}
