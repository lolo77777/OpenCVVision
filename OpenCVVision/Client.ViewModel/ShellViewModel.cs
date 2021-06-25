using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Splat;

using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Client.ViewModel
{
    public class ShellViewModel : ViewModelBase, IRoutableViewModel, IScreen
    {
        private readonly IReadonlyDependencyResolver _resolver = Locator.Current;
        public ImageViewModel ImageVMSam { get; private set; }
        public NavigationViewModel NavigationViewModelSam { get; private set; }
        public RoutingState Router { get; } = new RoutingState();
        [Reactive] public string TxtUi { get; set; } = "hello";
        public string UrlPathSegment { get; } = "ShellView";
        public IScreen HostScreen { get; }

        public ShellViewModel(NavigationViewModel navigationViewModel = null, ImageViewModel imageViewModel = null, IScreen screen = null) : base()
        {
            HostScreen = screen ?? _resolver.GetService<IScreen>("MainHost");
            Locator.CurrentMutable.RegisterConstant<IScreen>(this, "OperationHost");
            NavigationViewModelSam = navigationViewModel ?? _resolver.GetService<NavigationViewModel>();
            ImageVMSam = imageViewModel ?? _resolver.GetService<ImageViewModel>();
        }

        protected override void SetupSubscriptions(CompositeDisposable d)
        {
            base.SetupSubscriptions(d);
            MessageBus.Current.Listen<NaviItem>()
                .Select(it => _resolver.GetService<IOperationViewModel>(it.OperaPanelInfo))
                .WhereNotNull()
                .Do(vm =>
                {
                    Router.Navigate.Execute(vm);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                })
                .Subscribe()
                .DisposeWith(d);
        }
    }
}