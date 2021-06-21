using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Common;

using Splat;

using Client.ViewModel;

using Client.ViewModel.Operation;

namespace Client.ViewModel
{
    internal class Register : RegisterBase
    {
        private void RegistLazySingletonOpVM<T>() where T : IOperationViewModel, new()
        {
            _mutable.Register<IOperationViewModel>(() => new T(), OpStaticMethod.GetOpInfo<T>().info);
        }

        public static T ResolveVM<T>() where T : IOperationViewModel
        {
            return (T)Locator.Current.GetService<IOperationViewModel>(OpStaticMethod.GetOpInfo<T>().info);
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
            RegistLazySingletonOpVM<LaserLineViewModel>();
            RegistLazySingletonOpVM<PyramidViewModel>();
            RegistLazySingletonOpVM<EdgeDetectViewModel>();
            RegistLazySingletonOpVM<YoloViewModel>();
            RegistLazySingletonOpVM<FeatureDetectionViewModel>();
            //RegistLazySingletonOpVM<MatchTemplateViewModel>();
            RegistLazySingletonOpVM<GrayCodeViewModel>();
            _mutable.Register(() => new View3DViewModel());
        }
    }
}