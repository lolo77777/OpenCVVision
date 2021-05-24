using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Common;
using Client.ViewModel;

using ReactiveUI;

using Splat;

namespace Client.View
{
    internal class Register : RegisterBase
    {
        public override void ConfigService()
        {
            _mutable.RegisterLazySingleton(() => new MainWindow());
            _mutable.RegisterLazySingleton<IViewFor<NavigationViewModel>>(() => new Navigation());
            _mutable.RegisterLazySingleton<IViewFor<ImageViewModel>>(() => new ImageView());
        }
    }
}