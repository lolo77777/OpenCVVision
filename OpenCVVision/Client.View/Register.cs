using Client.Common;
using Client.View.Operation;

using Client.ViewModel;
using Client.ViewModel.Operation;

using ReactiveUI;

using Splat;

namespace Client.View
{
    internal class Register : RegisterBase
    {
        private void RegisterVLazySingleton<T1, T2>() where T1 : OperaViewModelBase where T2 : IViewFor<T1>, new()
        {
            _mutable.Register<IViewFor<T1>>(() => new T2());
        }

        public override void ConfigService()
        {
            _mutable.RegisterLazySingleton<IViewFor<ShellViewModel>>(() => new ShellView());
            _mutable.RegisterLazySingleton<IViewFor<NavigationViewModel>>(() => new Navigation());
            _mutable.RegisterLazySingleton<IViewFor<ImageViewModel>>(() => new ImageView());

            RegisterVLazySingleton<LoadFileViewModel, LoadFileView>();
            RegisterVLazySingleton<ColorSpaceViewModel, ColorSpaceView>();
            RegisterVLazySingleton<FilterViewModel, FilterView>();

            RegisterVLazySingleton<ThreshouldViewModel, ThresholdView>();
            RegisterVLazySingleton<MorphologyViewModel, MorphologyView>();
            RegisterVLazySingleton<ConnectedComponentsViewModel, ConnectedComponentsView>();
            RegisterVLazySingleton<ContoursViewModel, ContoursView>();
            RegisterVLazySingleton<RoiViewModel, RoiView>();
            RegisterVLazySingleton<LaserLineViewModel, LaserLineView>();
            RegisterVLazySingleton<PyramidViewModel, PyramidView>();
            RegisterVLazySingleton<EdgeDetectViewModel, EdgeDetectView>();
            RegisterVLazySingleton<YoloViewModel, YoloView>();
            RegisterVLazySingleton<FeatureDetectionViewModel, FeatureDetectionView>();
            //RegisterVLazySingleton<MatchTemplateViewModel, MatchTemplateView>();
            RegisterVLazySingleton<GrayCodeViewModel, GrayCodeView>();
            _mutable.Register<IViewFor<View3DViewModel>>(() => new View3DView());
        }
    }
}