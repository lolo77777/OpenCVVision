using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Common;
using Client.View.Operation._01File;
using Client.View.Operation.Op02ColorSpace;
using Client.View.Operation.Op03PreProcessing;
using Client.ViewModel;
using Client.ViewModel.Operation.Op01File;
using Client.ViewModel.Operation.Op02ColorSpace;
using Client.ViewModel.Operation.Op03PreProcessing;

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
            _mutable.RegisterLazySingleton<IViewFor<LoadFileViewModel>>(() => new LoadFileView());
            _mutable.RegisterLazySingleton<IViewFor<ColorSpaceViewModel>>(() => new ColorSpaceView());
            _mutable.RegisterLazySingleton<IViewFor<FilterViewModel>>(() => new FilterView());
        }
    }
}