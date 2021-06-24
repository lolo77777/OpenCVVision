using Client.Common;
using Client.ViewModel;

using Splat;

using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

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