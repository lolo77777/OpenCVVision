using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Common;

using Splat;

using Client.ViewModel;

namespace Client.ViewModel
{
    internal class Register : RegisterBase
    {
        public override void ConfigService()
        {
            _mutable.RegisterLazySingleton(() => new MainViewModel());
            _mutable.RegisterLazySingleton(() => new NavigationViewModel());
            _mutable.RegisterLazySingleton(() => new ImageViewModel());
        }
    }
}