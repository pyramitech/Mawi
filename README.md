# Mawi

> The framework code is written by hand and used in production. AI assisted with the documentation and with preparing the public release. Primary motivation was poor Android native MAUI performance and UI breaking between iOS releases.

A UI framework for .NET MAUI apps that renders the entire interface into a single platform WebView instead of native controls. You keep C# and XAML — Mawi keeps a retained view tree in C#, serializes changes into batched DOM updates, and ships them to the WebView in one eval per frame. On Android, DOM construction outperforms native MAUI element construction by more than 10×.

Mawi views mirror the MAUI API surface (`WLabel`, `WGrid`, `HorizontalOptions`, …) so existing XAML ports over with minimal changes.

## Installation

```
dotnet add package Mawi
```

Mawi works in .NET 9 MAUI apps targeting Android, iOS, Mac Catalyst and Windows. You don't need to register anything in `MauiProgram`, but the app must use the invariant culture (see [Culture](#culture)).

## Getting started

Host a `WRoot` anywhere in a MAUI page:

```xml
<ContentPage xmlns:mawi="clr-namespace:Mawi;assembly=Mawi" ...>
    <mawi:WRoot x:Name="RootView"/>
</ContentPage>
```

Then assign it content — either from XAML or with the fluent C# API:

```csharp
using Mawi.Fluent;
using Mawi.Views;

int clicks = 0;
var count = new WLabel { Text = "0" };
var button = new WButton { Text = "Click me" }
    .BackgroundColor(Colors.SlateBlue).BorderRadius(8).Padding(14, 9);
button.Click += (s, e) => count.Text = $"{++clicks}";

RootView.Content = new WStackLayout
{
    Children =
    {
        new WLabel { Text = "Hello, Mawi" }.FontSize(24).Bold(),
        count,
        button,
    }
}.Spacing(12).Padding(24);
```

For a full app, see [`samples/Mawi.Gallery`](samples/Mawi.Gallery).

Mutating any property re-renders only the affected element; updates are batched automatically.

## Views

| Category   | Views |
|------------|-------|
| Text       | `WLabel` (spans via `WFormattedString`), `WEntry`, `WEditor` |
| Controls   | `WButton`, `WImage`, `WBoxView`, `WActivityIndicator`, `WHtml` |
| Layout     | `WStackLayout`, `WGrid`, `WFlexLayout`, `WScrollLayout`, `WFrame` (single-child wrapper), `WContent` |
| Navigation | `WShell`, `WNavigationView` |
| Data       | `WVirtualList` |

## Data binding

`{w:Bind}` binds to plain CLR properties — no `BindableProperty` machinery, zero cost when unused. Supports `INotifyPropertyChanged` (updates from any thread are marshaled to the main thread), two-way binding on input controls, `StringFormat`, converters, and multi-hop paths.

```xml
<w:WEntry Text="{w:Bind Name}"/>
<w:WLabel Text="{w:Bind Name, StringFormat='Hello, {0}!'}"/>
<w:WLabel Text="{w:Bind Count, Converter={x:Static local:MyConverters.Upper}}"/>
```

`BindingContext` cascades down the tree as in MAUI.

## Navigation

`WShell` maps routes to page templates and animates page mounts:

```csharp
var shell = new WShell().Fill();
shell.AddPage("home", () => BuildHomePage());
shell.AddPage("settings", () => BuildSettingsPage());
await shell.NavigateTo("home");
```

## Virtualized lists

`WVirtualList` handles large sources with variable item heights: it measures the items it renders, estimates the rest from their average, and keeps a windowed set of live elements while scrolling.

```csharp
var list = new WVirtualList
{
    ItemsSource = items,                       // any IList
    ItemTemplate = item => BuildRow(item),     // item is set as BindingContext
    EstimatedItemSize = 44,
}.Fill();

list.ScrollToIndex(5000);
```

## Animations

Property animations run on the browser's compositor via the Web Animations API:

```csharp
await box.TranslateToAsync(200, 0, 400, WEasing.EaseInOut);
```

For multi-property sequences, build keyframes and await completion in C#:

```csharp
var anim = new WAnimation { Duration = 900, Fill = FillMode.Both, Easing = WEasing.EaseInOut };
anim.Keyframes.Add(new WKeyframe().AddProperty<WView>(v => v.Rotation, 0d));
anim.Keyframes.Add(new WKeyframe().Offset(0.5).AddProperty<WView>(v => v.Rotation, 180d));
anim.Keyframes.Add(new WKeyframe().AddProperty<WView>(v => v.Rotation, 360d));
await box.AnimateAsync(anim);
```

## Root services

`WRoot` also provides document-level services: global CSS (`WRoot.DefaultStyle`, `LoadStyle`/`UnloadStyle`), scrollbar theming (`SetScrollbarTheme` with a typed `WScrollbarTheme`), custom fonts (`AddFont`), image caching (`LoadImageFromFile` / `LoadImageFromData` / `LoadImageFromResources`), and raw script injection (`LoadScript`).

## Threading

The view tree is main-thread-only; mutate views on the main thread (watch for `await` continuations landing on other threads, e.g. on iOS). Binding updates arriving from background threads are marshaled automatically.

## Culture

Mawi requires the invariant culture for number formatting. Numeric values such as scale, rotation, flex grow/shrink and animation keyframe points are written into CSS with the thread's current culture, and CSS only accepts `.` as the decimal separator. Set the invariant culture at startup, before any Mawi view is created:

```csharp
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
```

Values coming back from the WebView (event arguments, measurements) are always parsed as invariant.

## Error reporting

Render and binding failures are reported through `WRoot.ErrorDebugAction`. When Mawi itself is compiled in the Debug configuration (building from source), off-thread mutations are reported there too; the NuGet package is a Release build without those checks.

## Debugging the WebView

WebView inspection is off by default. Opt in per root, for example only in debug builds:

```csharp
#if DEBUG
RootView.Settings = new RootSettings { EnableDevTools = true };
#endif
```

This enables the WebView2 DevTools on Windows and the Safari Web Inspector on iOS and Mac Catalyst. On Android, call `Android.Webkit.WebView.SetWebContentsDebuggingEnabled(true)` in your app to use `chrome://inspect`.

## Platforms

.NET 9: Android (API 21+), iOS 14+, Mac Catalyst 15+, Windows 10 1809+ (WebView2).

## Repository layout

- `src/Mawi/`: the framework (the `Mawi` NuGet package)
- `samples/Mawi.Gallery/`: gallery app showcasing layouts, text, inputs, binding, navigation, virtualized lists, and animations
- `pack.ps1`: builds the NuGet package into `artifacts/`

## License

Licensed under the [Apache License 2.0](LICENSE).
