namespace OpenCVVision.ViewModel;

public class OperaViewModelBase : ViewModelBase, IOperationViewModel
{
    internal IImageDataManager _imageDataManager;
    internal IReadonlyDependencyResolver _resolver = Locator.Current;

    /// <summary>
    /// mat标记，方便释放
    /// </summary>
    internal ResourcesTracker _rt = new();

    internal Mat _sigleSrc;
    internal Mat _src;

    /// <summary>
    /// 标记操作进行状态
    /// </summary>
    [Reactive] protected bool IsRun { set; get; } = false;

    /// <summary>
    /// 标记能否进行操作
    /// </summary>
    [Reactive] public bool CanOperat { get; set; }

    public OperaViewModelBase(IImageDataManager imageDataManager = null) : base()
    {
        _imageDataManager = imageDataManager ?? _resolver.GetService<IImageDataManager>();
    }

    /// <summary>
    /// 执行操作，发送操作执行的时间
    /// </summary>
    /// <param name="action">图像的操作</param>
    protected void SendTime(Action action)
    {
        if (!IsRun)
        {
            IsRun = true;
            long t1 = Cv2.GetTickCount();
            var currentMat = _imageDataManager.GetCurrentMat();
            if (currentMat != null)
            {
                _src = _rt.T(currentMat.Clone());
            }
            else
            {
                return;
            }
            _sigleSrc = _rt.T(_src.Channels() > 1 ? _src.CvtColor(ColorConversionCodes.BGR2GRAY) : _src);
            MessageBus.Current.SendMessage("Wait...", "Time");

            var ob = Observable.Start(action, RxApp.TaskpoolScheduler);

            ob.Subscribe(_ => { }, () =>
            {
                _rt.Dispose();
                long t2 = Cv2.GetTickCount();
                double t = Math.Round((t2 - t1) / Cv2.GetTickFrequency() * 1000, 0);
                MessageBus.Current.SendMessage(t.ToString(), "Time");
                IsRun = false;
            });
        }
    }

    /// <summary>
    /// 执行操作，发送操作执行的时间
    /// </summary>
    /// <param name="action">图像的操作</param>
    protected void SendTime(Action action, bool isWait)
    {
        if (!IsRun)
        {
            IsRun = true;
            long t1 = Cv2.GetTickCount();
            var currentMat = _imageDataManager.GetCurrentMat();
            if (currentMat != null)
            {
                _src = _rt.T(currentMat.Clone());
            }
            else
            {
                return;
            }

            _sigleSrc = _rt.T(_src.Channels() > 1 ? _src.CvtColor(ColorConversionCodes.BGR2GRAY) : _src);
            MessageBus.Current.SendMessage("Wait...", "Time");

            var ob = Observable.Start(action, RxApp.TaskpoolScheduler);
            if (isWait)
            {
                ob.Wait();
            }
            ob.Subscribe(_ => { }, () =>
            {
                _rt.Dispose();
                long t2 = Cv2.GetTickCount();
                double t = Math.Round((t2 - t1) / Cv2.GetTickFrequency() * 1000, 0);
                MessageBus.Current.SendMessage(t.ToString(), "Time");
                IsRun = false;
            });
        }
    }

    /// <summary>
    /// 设置启动时加载
    /// </summary>
    protected override void SetupStart()
    {
        base.SetupStart();
        _imageDataManager.RaiseCurrent();
    }

    /// <summary>
    /// 设置流订阅
    /// </summary>
    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        _imageDataManager.InputMatGuidSubject
            .Select(guid => guid != null)
            .BindTo(this, x => x.CanOperat)
            .DisposeWith(d);
    }

    protected override void SetupDeactivate()
    {
        base.SetupDeactivate();
        _rt.Dispose();
    }
}