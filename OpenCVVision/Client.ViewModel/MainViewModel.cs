using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Client.Common;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Splat;

namespace Client.ViewModel
{
    public class MainViewModel : ReactiveObject, IScreen
    {
        private readonly IReadonlyDependencyResolver _resolver = Locator.Current;
        public ImageViewModel ImageVMSam { get; private set; }
        public NavigationViewModel NavigationViewModelSam { get; private set; }
        public RoutingState Router { get; } = new RoutingState();
        [Reactive] public string TxtUi { get; set; } = "hello";

        public MainViewModel(NavigationViewModel navigationViewModel = null, ImageViewModel imageViewModel = null)
        {
            navigationViewModel ??= _resolver.GetService<NavigationViewModel>();
            imageViewModel ??= _resolver.GetService<ImageViewModel>();
            NavigationViewModelSam = navigationViewModel;
            ImageVMSam = imageViewModel;
            SetupSubscriptions();
        }

        private void SetupSubscriptions()
        {
            MessageBus.Current.Listen<NaviItem>()
                     .Select(it => _resolver.GetService<IOperationViewModel>(it.OperaPanelInfo))
                     .WhereNotNull()
                     .Do(vm => { Router.Navigate.Execute(vm); GC.Collect(); })
                     .Subscribe();
        }
    }
}