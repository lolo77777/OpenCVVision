using Client.Common;

using ReactiveUI;

using Splat;

using System;
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
            Locator.CurrentMutable.RegisterConstant<IScreen>(this, "MainHost");
            Router.Navigate.Execute(Locator.Current.GetService<ShellViewModel>());
        }

        private void LoadDlls()
        {
            var starupPath = Environment.CurrentDirectory;

            var dlls = Directory.GetFiles(starupPath).Where(f => f.Contains(".dll")).Select(f => f.Replace(starupPath + @"\", "")).Where(n => n.Contains("Client")).ToList();
            foreach (var dll in dlls)
            {
                var tmp = (
                    from t in Assembly.LoadFrom(dll).GetTypes()
                    where t.IsSubclassOf(typeof(RegisterBase))
                    select Activator.CreateInstance(t)).ToList();


                tmp.Clear();

            }
        }
    }
}