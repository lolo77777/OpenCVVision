using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            _mutable.RegisterLazySingleton(() => new MainWindow());
            _mutable.RegisterLazySingleton<IViewFor<NavigationViewModel>>(() => new Navigation());
            _mutable.RegisterLazySingleton<IViewFor<ImageViewModel>>(() => new ImageView());
            //_mutable.RegisterLazySingleton<IViewFor<LoadFileViewModel>>(() => new LoadFileView());
            //_mutable.RegisterLazySingleton<IViewFor<ColorSpaceViewModel>>(() => new ColorSpaceView());
            //_mutable.RegisterLazySingleton<IViewFor<FilterViewModel>>(() => new FilterView());
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
            RegisterVLazySingleton<YoloV3ViewModel, YoloV3View>();
        }
    }
}