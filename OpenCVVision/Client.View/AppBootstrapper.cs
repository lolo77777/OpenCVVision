using Client.Common;

using NLog;

using ReactiveUI;

using Splat;
using Splat.NLog;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Client.ViewModel
{
    public class AppBootstrapper : ReactiveObject, IScreen
    {
        public RoutingState Router { get; private set; } = new RoutingState();


        public AppBootstrapper()
        {
            LoadDlls();
            ConfigLog();
            Locator.CurrentMutable.RegisterConstant<IScreen>(this, "MainHost");
            Router.Navigate.Execute(Locator.Current.GetService<ShellViewModel>());
        }

        private void LoadDlls()
        {
            string starupPath = Environment.CurrentDirectory;
            List<string> dlls = Directory.GetFiles(starupPath).Where(f => f.Contains(".dll")).Select(f => f.Replace(starupPath + @"\", "")).Where(n => n.Contains("Client")).ToList();
            foreach (string dll in dlls)
            {
                List<object> tmp = (
                    from t in Assembly.LoadFrom(dll).GetTypes()
                    where t.IsSubclassOf(typeof(RegisterBase))
                    select Activator.CreateInstance(t)).ToList();
                tmp.Clear();
            }
        }
        private void ConfigLog()
        {
            LogManager.GetCurrentClassLogger();
            Locator.CurrentMutable.UseNLogWithWrappingFullLogger();
            RxApp.SuppressViewCommandBindingMessage = true;
        }
    }
}