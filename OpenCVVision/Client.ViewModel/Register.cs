using Client.Common;
using Client.ViewModel.Operation;

using Splat;

namespace Client.ViewModel
{
    internal class Register : RegisterBase
    {
        private void RegistOperationViewModel<T>() where T : IOperationViewModel, new()
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
            RegistOperationViewModel<LoadFileViewModel>();
            RegistOperationViewModel<ColorSpaceViewModel>();
            RegistOperationViewModel<FilterViewModel>();

            RegistOperationViewModel<ThreshouldViewModel>();
            RegistOperationViewModel<MorphologyViewModel>();
            RegistOperationViewModel<ConnectedComponentsViewModel>();
            RegistOperationViewModel<ContoursViewModel>();
            RegistOperationViewModel<RoiViewModel>();
            RegistOperationViewModel<LaserLineViewModel>();
            RegistOperationViewModel<PyramidViewModel>();
            RegistOperationViewModel<EdgeDetectViewModel>();
            RegistOperationViewModel<YoloViewModel>();
            RegistOperationViewModel<FeatureDetectionViewModel>();
            //RegistLazySingletonOpVM<MatchTemplateViewModel>();
            RegistOperationViewModel<GrayCodeViewModel>();
            _mutable.Register(() => new View3DViewModel());
        }
    }
}