

namespace Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IEnableLogger
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            SplashScreen s = new SplashScreen("Vison.ico");

            s.Show(false);
            s.Close(TimeSpan.FromMilliseconds(100));
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        private void DeviceClose()
        {
            //退出程序时检查相机是否释放
            var cams = Locator.Current.GetServices<ICamera>();
            foreach (var cam in cams)
            {
                if (cam != null)
                {
                    cam.CloseDevices();
                    cam.Dispose();
                }
            }

            this.Log().Info("*******************");
            this.Log().Info("******系统退出******");
            this.Log().Info("*******************");
        }
    }
}