using Client.Common;
using Client.ViewModel.Operation;

using Splat;

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
            _mutable.RegisterLazySingleton(() => new ShellViewModel());
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