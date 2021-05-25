using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Common;

using Splat;

using Client.ViewModel;
using Client.ViewModel.Operation.Op01File;
using Client.ViewModel.Operation.Op02ColorSpace;
using Client.ViewModel.Operation.Op03PreProcessing;

namespace Client.ViewModel
{
    internal class Register : RegisterBase
    {
        public override void ConfigService()
        {
            _mutable.RegisterLazySingleton(() => new MainViewModel());
            _mutable.RegisterLazySingleton(() => new NavigationViewModel());
            _mutable.RegisterLazySingleton(() => new ImageViewModel());
            _mutable.RegisterLazySingleton<IOperationViewModel>(() => new LoadFileViewModel(), StaticMethod.GetInfo(typeof(LoadFileViewModel)));
            _mutable.RegisterLazySingleton<IOperationViewModel>(() => new ColorSpaceViewModel(), StaticMethod.GetInfo(typeof(ColorSpaceViewModel)));
            _mutable.RegisterLazySingleton<IOperationViewModel>(() => new FilterViewModel(), StaticMethod.GetInfo(typeof(FilterViewModel)));
        }
    }
}