namespace Client.ViewModel;

[SingleInstanceView]
public class ShellViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly IReadonlyDependencyResolver _resolver = Locator.Current;
    public ImageViewModel ImageVMSam { get; private set; }
    public NavigationViewModel NavigationViewModelSam { get; private set; }

    [Reactive] public IOperationViewModel OperaVM { get; set; }
    public string UrlPathSegment { get; }
    public IScreen HostScreen { get; }

    public ShellViewModel(NavigationViewModel navigationViewModel = null, ImageViewModel imageViewModel = null) : base()
    {
        NavigationViewModelSam = navigationViewModel ?? _resolver.GetService<NavigationViewModel>();
        ImageVMSam = imageViewModel ?? _resolver.GetService<ImageViewModel>();
    }

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        base.SetupSubscriptions(d);
        MessageBus.Current.Listen<NaviItem>()
            .Select(it => _resolver.GetService<IOperationViewModel>(it.OperaPanelInfo))
            .WhereNotNull()
            .Subscribe(vm =>
            {
                OperaVM = vm;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            })
            .DisposeWith(d);
    }
}