namespace Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            SplashScreen s = new SplashScreen("Vison.ico");

            s.Show(false);
            s.Close(TimeSpan.FromMilliseconds(100));
            base.OnStartup(e);
        }
    }
}