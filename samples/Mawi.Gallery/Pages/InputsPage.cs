using Mawi.Fluent;
using Mawi.Views;
using static Mawi.Gallery.Ui;

namespace Mawi.Gallery.Pages
{
    internal static class InputsPage
    {
        public static WView Build()
        {
            var echo = new WLabel { Text = "type something…", TextColor = InkSoft, FontSize = 14 };

            var entry = Field("regular entry, events round-trip live");
            entry.TextChanged += (s, e) => echo.Text = e.Value?.Length > 0 ? $"you typed: {e.Value}" : "type something…";

            var password = Field("password entry");
            password.IsPassword = true;

            var numeric = Field("numeric keyboard on touch devices");
            numeric.Keyboard = WKeyboard.Numeric;

            var editor = new WEditor
            {
                Text = "A multiline editor.\nInitial text with line breaks survives the textarea content model."
                    + string.Concat(Enumerable.Range(1, 12).Select(i => $"\nline {i + 2} — enough content to overflow the fixed height, so the editor scrolls.")),
                TextColor = Ink,
                FontSize = 14,
                LineBreakMode = LineBreakMode.WordWrap,
            }.BackgroundColor(FieldBg).BorderRadius(8).Padding(12, 10).Height(110).FillHorizontal();

            var focusBtn = PrimaryButton("Focus the first entry").Start();
            focusBtn.Click += (s, e) => entry.Focus();

            var lockedEntry = new WEntry
            {
                Text = "can't touch this",
                TextColor = Ink,
                FontSize = 15,
                IsEnabled = false,
            }.BackgroundColor(FieldBg).BorderRadius(8).Padding(12, 10);

            var lockedBtn = PrimaryButton("Disabled button").Start();
            lockedBtn.IsEnabled = false;
            lockedBtn.Click += (s, e) => lockedBtn.Text = "this should never happen";

            var toggleBtn = PrimaryButton("Enable them").BackgroundColor(Ink).Start();
            toggleBtn.Click += (s, e) =>
            {
                var enable = !lockedEntry.IsEnabled;
                lockedEntry.IsEnabled = enable;
                lockedBtn.IsEnabled = enable;
                toggleBtn.Text = enable ? "Disable them" : "Enable them";
            };

            var disabledRow = new WStackLayout { Orientation = WStackOrientation.Horizontal, Spacing = 10 };
            disabledRow.Children.Add(lockedBtn);
            disabledRow.Children.Add(toggleBtn);

            return Page(
                PageTitle("Inputs"),
                Card("Entries", entry, password, numeric, echo),
                Card("Editor", editor),
                Card("Disabled state — styled via the [disabled] css hook", lockedEntry, disabledRow),
                Card("Programmatic focus", focusBtn));
        }
    }
}
