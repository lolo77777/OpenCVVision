using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

using Client.Common;

using Splat;

namespace Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Init()
        {
            var starupPath = Environment.CurrentDirectory;

            var dlls = Directory.GetFiles(starupPath).Where(f => f.Contains(".dll")).Select(f => f.Replace(starupPath + @"\", "")).Where(n => n.Contains("Client")).ToList();
            foreach (var dll in dlls)
            {
                var tmp = (from t in Assembly.LoadFrom(dll).GetTypes()
                           where t.IsSubclassOf(typeof(RegisterBase))
                           select Activator.CreateInstance(t)).ToList();
                tmp.Clear();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Init();
            Locator.Current.GetService<MainWindow>().Show();
        }
    }
}