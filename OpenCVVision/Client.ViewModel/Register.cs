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
using Client.ViewModel.Operation;

namespace Client.ViewModel
{
    internal class Register : RegisterBase
    {
        private void RegistLazySingletonOpVM<T>() where T : IOperationViewModel, new()
        {
            _mutable.Register<IOperationViewModel>(() => new T(), StaticMethod.GetInfo<T>());
        }

        public static T ResolveVM<T>() where T : IOperationViewModel, new()
        {
            return (T)Locator.Current.GetService<IOperationViewModel>(StaticMethod.GetInfo<T>());
        }

        public override void ConfigService()
        {
            _mutable.RegisterLazySingleton(() => new MainViewModel());
            _mutable.RegisterLazySingleton(() => new NavigationViewModel());
            _mutable.RegisterLazySingleton(() => new ImageViewModel());

            RegistLazySingletonOpVM<LoadFileViewModel>();
            RegistLazySingletonOpVM<ColorSpaceViewModel>();
            RegistLazySingletonOpVM<FilterViewModel>();

            RegistLazySingletonOpVM<ThreshouldViewModel>();
            RegistLazySingletonOpVM<MorphologyViewModel>();
            RegistLazySingletonOpVM<ConnectedComponentsViewModel>();
            RegistLazySingletonOpVM<ContoursViewModel>();
            RegistLazySingletonOpVM<RoiViewModel>();
        }
    }
}