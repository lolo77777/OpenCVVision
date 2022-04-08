namespace Client.ViewModel.Operation;

[OperationInfo(3, "滤波", MaterialDesignThemes.Wpf.PackIconKind.Filter)]
public class FilterViewModel : OperaViewModelBase
{
    [ObservableAsProperty] public bool BolSigmaColorAndSpace { get; set; }
    [ObservableAsProperty] public bool BolSigmaIsEnable { get; set; }
    [ObservableAsProperty] public bool BolSizeIsEnable { get; set; }
    [ObservableAsProperty] public bool BolSizeYIsEnable { get; set; }
    public IList<string> FilterModes { get; private set; }
    [Reactive] public int FilterModeSelectIndex { get; set; }
    [Reactive] public int KernelDiam { get; set; }
    [Reactive] public double SigmaColor { get; set; }
    [Reactive] public double SigmaSpace { get; set; }
    [Reactive] public double SigmaX { get; set; }
    [Reactive] public double SigmaY { get; set; }
    [Reactive] public int SizeX { get; private set; } = 3;
    [Reactive] public int SizeY { get; private set; } = 3;
    [Reactive] public float Factor { get; set; } = 1;

    private void UpdateUi(int filterModeSelectIndex, int kernelSizeX, int kernelSizeY, double sigmaX, double sigmaY, int kernelDiam, double sigmaColor, double sigmaSpace)
    {
        SendTime(() =>
        {
            if (filterModeSelectIndex > 3)
            {
                Mat dst = _rt.NewMat();
                dst = _src.BilateralFilter(kernelDiam, sigmaColor, sigmaSpace);
                _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
            }
            else
            {
                Mat dst = _rt.NewMat();
                dst = filterModeSelectIndex switch
                {
                    0 => _src.Blur(new Size(kernelSizeX, kernelSizeY)),
                    1 => _src.GaussianBlur(new Size(kernelSizeX, kernelSizeY), sigmaX, sigmaY),
                    2 => _src.MedianBlur(kernelSizeX),
                    3 => _src.EmphasizeEx(SizeX, SizeY, Factor),
                    _ => _src.Clone()
                };
                _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
            }
        });
    }

    protected override void SetupStart()
    {
        base.SetupStart();
        FilterModes = new[] { "Blur", "Gaussian", "Median", "Emphasize(Halcon)", "BilateralFilter" };
    }

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        base.SetupSubscriptions(d);
        this.WhenAnyValue(x => x.FilterModeSelectIndex)
            .Select(i => i.Equals(1))
            .ToPropertyEx(this, x => x.BolSigmaIsEnable)
            .DisposeWith(d);
        this.WhenAnyValue(x => x.FilterModeSelectIndex)
            .Select(i => !i.Equals(2))
            .ToPropertyEx(this, x => x.BolSizeYIsEnable)
            .DisposeWith(d);
        this.WhenAnyValue(x => x.FilterModeSelectIndex)
            .Select(i => i.Equals(4))
            .ToPropertyEx(this, x => x.BolSigmaColorAndSpace)
            .DisposeWith(d);
        this.WhenAnyValue(x => x.FilterModeSelectIndex)
            .Select(i => !i.Equals(4))
            .ToPropertyEx(this, x => x.BolSizeIsEnable)
            .DisposeWith(d);
        this.WhenAnyValue(x => x.FilterModeSelectIndex, x => x.SizeX, x => x.SizeY, x => x.SigmaX, x => x.SigmaY, x => x.Factor)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .Where(vt => CanOperat && vt.Item1 < 4 && vt.Item1 >= 0 && vt.Item2 > 0 && vt.Item3 > 0)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(vt => UpdateUi(FilterModeSelectIndex, SizeX, SizeY, SigmaX, SigmaY, KernelDiam, SigmaColor, SigmaSpace))
            .DisposeWith(d);
        this.WhenAnyValue(x => x.FilterModeSelectIndex, x => x.KernelDiam, x => x.SigmaColor, x => x.SigmaSpace)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .Where(vt => CanOperat && vt.Item1.Equals(4) && vt.Item2 > 0 && vt.Item3 > 0)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(vt => UpdateUi(FilterModeSelectIndex, SizeX, SizeY, SigmaX, SigmaY, KernelDiam, SigmaColor, SigmaSpace))
            .DisposeWith(d);
        _imageDataManager.InputMatGuidSubject
            .WhereNotNull()
            .Where(guid => CanOperat)
            .Log(this, "图像触发选择更新")
            .Subscribe(guid => UpdateUi(FilterModeSelectIndex, SizeX, SizeY, SigmaX, SigmaY, KernelDiam, SigmaColor, SigmaSpace))
            .DisposeWith(d);
    }
}