using OpenCVVision.Contracts;
using OpenCVVision.Model.Entity;

namespace OpenCVVision.ViewModel;

[SingleInstanceView]
public class ShellViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly IReadonlyDependencyResolver _resolver = Locator.Current;

    private ReadOnlyObservableCollection<DisplayLogInfo> _logDataInfo;
    private readonly IDisplayLog _displayLog;
    public ReadOnlyObservableCollection<DisplayLogInfo> LogDataInfo => _logDataInfo;
    public ImageViewModel ImageVMSam { get; private set; }
    public NavigationViewModel NavigationViewModelSam { get; private set; }
    [Reactive] public string LogMsg { get; set; } = "信息:";
    [Reactive] public Splat.LogLevel MsgLogLevel { get; set; }
    [Reactive] public IOperationViewModel OperaVM { get; set; }
    public string UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    [Reactive] public int LevelSelectIndex { get; set; }

    public ShellViewModel(NavigationViewModel navigationViewModel = null, ImageViewModel imageViewModel = null, IDisplayLog displayLog = null) : base()
    {
        NavigationViewModelSam = navigationViewModel ?? _resolver.GetService<NavigationViewModel>();
        ImageVMSam = imageViewModel ?? _resolver.GetService<ImageViewModel>();
        _displayLog = displayLog ?? Locator.Current.GetService<IDisplayLog>();
    }

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        base.SetupSubscriptions(d);
        ObservableLogger.LogMsg
            .WhereNotNull()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(vt =>
            {
                LogMsg = $"信息:{DateTime.Now.ToShortTimeString()}:{vt.msg}";
                MsgLogLevel = vt.level;
            }).DisposeWith(d);
        MessageBus.Current.Listen<NaviItemStr>()
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
        _displayLog.LogCache
            .Connect()
            .Filter(t => t.LogLevel > (LogLevel)LevelSelectIndex + 1)
            .Transform(t => new DisplayLogInfo(t))
            .SortBy(t => t.Id, DynamicData.Binding.SortDirection.Descending)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _logDataInfo)
            .Subscribe()
            .DisposeWith(d);
        this.WhenAnyValue(x => x.LevelSelectIndex)
            .Where(i => i > -1)
            .Subscribe(_ => _displayLog.LogCache.Refresh())
            .DisposeWith(d);
    }

    protected override void SetupStart()
    {
        base.SetupStart();

        _displayLog.LogCache.Refresh();
    }
}