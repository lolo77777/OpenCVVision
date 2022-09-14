namespace OpenCVVision.ViewModel.Operation;

[OperationInfo(2, "色彩空间", "Color")]
public class ColorSpaceViewModel : OperaViewModelBase
{
    [ObservableAsProperty] public IList<int> Channels { get; set; }
    [Reactive] public int ChannelSelectInd { get; set; }
    public IList<string> ColorModes { get; private set; }
    [Reactive] public int ColorModeSelectInd { get; set; }
    [Reactive] public bool IsEnableInverse { get; set; }

    private void UpdateOutput(int colorModeInd, int channel)
    {
        this.Log().Debug("色彩转换");
        SendTime(() =>
        {
            if (!_src.Channels().Equals(1))
            {
                Mat dst = _rt.NewMat();
                dst = colorModeInd switch
                {
                    0 => _src.CvtColor(ColorConversionCodes.BGR2GRAY),
                    1 => channel.Equals(0) ? _src : _src.Split()[channel - 1],
                    2 => channel.Equals(0) ? _src : _src.CvtColor(ColorConversionCodes.BGR2HSV).Split()[channel - 1],
                    3 => channel.Equals(0) ? _src : _src.CvtColor(ColorConversionCodes.BGR2HLS).Split()[channel - 1],
                    _ => _src.Clone()
                };

                _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
            }
        });
    }

    private void UpdateOutputInv(bool isInv)
    {
        SendTime(() =>
        {
            Mat dst = _rt.NewMat();
            dst = _src;
            if (isInv)
            {
                Cv2.BitwiseNot(_sigleSrc, dst);
            }
            _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
        });
    }

    protected override void SetupStart()
    {
        base.SetupStart();
        ColorModes = new[] { "Gray", "BGR", "HSV", "HLS" };
    }

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        base.SetupSubscriptions(d);

        this.WhenAnyValue(x => x.ColorModeSelectInd)
            .Where(i => i >= 0)
            .Select(i => i.Equals(0) ? (new[] { 0 }) : Enumerable.Range(-1, 4).ToArray())
            .ToPropertyEx(this, x => x.Channels)
            .DisposeWith(d);

        this.WhenAnyValue(x => x.ColorModeSelectInd, x => x.ChannelSelectInd)
            .Throttle(TimeSpan.FromMilliseconds(70))
            .Where(i => i.Item1 >= 0 && i.Item2 >= 0 && Channels != null && Channels.Any())
            .Where(guid => CanOperat)
            .Subscribe(i => UpdateOutput(i.Item1, i.Item2))
            .DisposeWith(d);

        _imageDataManager.InputMatGuidSubject
            .WhereNotNull()
            .Where(guid => CanOperat)
            .Subscribe(guid => UpdateOutput(ColorModeSelectInd, ChannelSelectInd))
            .DisposeWith(d);

        _imageDataManager.InputMatGuidSubject
            .WhereNotNull()
            .Select(guid => _imageDataManager.GetCurrentMat().Channels() > 1)
            .BindTo(this, x => x.CanOperat)
            .DisposeWith(d);
        this.WhenAnyValue(x => x.IsEnableInverse)
            .Subscribe(bol => UpdateOutputInv(bol))
            .DisposeWith(d);
        //_imageDataManager.RaiseCurrent();
    }
}