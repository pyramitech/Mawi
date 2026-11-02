namespace Mawi.Gallery
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            //the whole gallery lives in one WRoot, no MAUI Shell chrome needed
            return new Window(new MainPage())
            {
                Title = "Mawi Gallery",
                Width = 1000,
                Height = 720,
            };
        }
    }
}
