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
        private void RegisterOperationView<T1, T2>() where T1 : OperaViewModelBase where T2 : IViewFor<T1>, new()
        {
            _mutable.Register<IViewFor<T1>>(() => new T2());
        }

        public override void ConfigService()
        {
            _mutable.RegisterLazySingleton<IViewFor<ShellViewModel>>(() => new ShellView());
            _mutable.RegisterLazySingleton<IViewFor<NavigationViewModel>>(() => new Navigation());
            _mutable.RegisterLazySingleton<IViewFor<ImageViewModel>>(() => new ImageView());

            RegisterOperationView<LoadFileViewModel, LoadFileView>();
            RegisterOperationView<ColorSpaceViewModel, ColorSpaceView>();
            RegisterOperationView<FilterViewModel, FilterView>();

            RegisterOperationView<ThreshouldViewModel, ThresholdView>();
            RegisterOperationView<MorphologyViewModel, MorphologyView>();
            RegisterOperationView<ConnectedComponentsViewModel, ConnectedComponentsView>();
            RegisterOperationView<ContoursViewModel, ContoursView>();
            RegisterOperationView<RoiViewModel, RoiView>();
            RegisterOperationView<LaserLineViewModel, LaserLineView>();
            RegisterOperationView<PyramidViewModel, PyramidView>();
            RegisterOperationView<EdgeDetectViewModel, EdgeDetectView>();
            RegisterOperationView<YoloViewModel, YoloView>();
            RegisterOperationView<FeatureDetectionViewModel, FeatureDetectionView>();
            //RegisterVLazySingleton<MatchTemplateViewModel, MatchTemplateView>();
            RegisterOperationView<GrayCodeViewModel, GrayCodeView>();
            _mutable.Register<IViewFor<View3DViewModel>>(() => new View3DView());
            RegisterOperationView<PhotometricStereoViewModel, PhotometricStereoView>();
        }
    }
}