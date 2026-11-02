using Mawi.Views;
using System.ComponentModel;
using System.Globalization;

namespace Mawi.Gallery.Pages;

public partial class BindingPage : WStackLayout
{
    public static readonly IValueConverter Upper = new UpperConverter();

    public BindingPage()
    {
        InitializeComponent();

        var vm = new BindingPageViewModel();
        BindingContext = vm;
        IncrementBtn.Click += (s, e) => vm.Count++;
    }

    private sealed class UpperConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value?.ToString()?.ToUpperInvariant();

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value;
    }
}

public sealed class BindingPageViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _name = "Mawi";
    private int _count;

    public string Name
    {
        get => _name;
        set { _name = value; PropertyChanged?.Invoke(this, new(nameof(Name))); }
    }

    public int Count
    {
        get => _count;
        set { _count = value; PropertyChanged?.Invoke(this, new(nameof(Count))); }
    }
}
