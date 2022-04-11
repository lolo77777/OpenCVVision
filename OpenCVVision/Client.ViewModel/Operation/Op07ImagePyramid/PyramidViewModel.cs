namespace Client.ViewModel.Operation;

[OperationInfo(7, "图像金字塔", "LanguagePython")]
public class PyramidViewModel : OperaViewModelBase
{
    public ReactiveCommand<Unit, Unit> LaplaceCommand;
    [Reactive] public int DownNum { get; set; }
    [Reactive] public int LaplaceNum { get; set; }
    [Reactive] public int UpNum { get; set; }
    [Reactive] public int UpNumMax { get; set; }

    protected override void SetupCommands()
    {
        base.SetupCommands();
        LaplaceCommand = ReactiveCommand.Create(DoLapace);
    }

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        base.SetupSubscriptions(d);
        this.WhenAnyValue(x => x.DownNum)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .Where(i => CanOperat)
            .Do(i => UpdateOutput(true, i))
            .Subscribe()
            .DisposeWith(d);
        this.WhenAnyValue(x => x.UpNum)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .Where(i => CanOperat)
            .Do(i => UpdateOutput(false, i))
            .Subscribe()
            .DisposeWith(d);
        this.WhenAnyValue(x => x.LaplaceNum)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .Where(i => CanOperat)
            .Do(i => DoLapaceNum(i))
            .Subscribe()
            .DisposeWith(d);
        _imageDataManager.InputMatGuidSubject
            .Where(guid => CanOperat)
            .Select(guid => _imageDataManager.GetCurrentMat().Size())
            .Select(size => GetUpnum(size))
            .BindTo(this, x => x.UpNumMax)
            .DisposeWith(d);
    }

    #region PrivateFunction

    private void DoLapace()
    {
        SendTime(() =>
        {
            Mat dstDown = _rt.NewMat();
            dstDown = DownMat(_src.Clone(), DownNum);
            Mat dstUp = _rt.NewMat();
            dstUp = UpMat(dstDown.Clone(), DownNum);
            Mat dst = _rt.NewMat();
            Mat srcNew = dstDown.Resize(dstUp.Size());
            dst = srcNew - dstUp;
            _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
        }, true);
    }

    private void DoLapaceNum(int num)
    {
        SendTime(() =>
        {
            var dstDown = _rt.NewMat();
            var dstDown1 = _rt.NewMat();
            dstDown = DownMat(_src.Clone(), num + 1);
            dstDown1 = DownMat(_src.Clone(), num);
            var dstUp = _rt.NewMat();
            dstUp = UpMat(dstDown.Clone(), num);
            var dst = _rt.NewMat();
            var srcNew = dstDown1.Resize(dstUp.Size());
            dst = srcNew - dstUp;
            _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
        }, true);
    }

    private Mat DownMat(Mat src, int num)
    {
        var dst = _rt.T(src.Clone());
        for (int i = 0; i < num; i++)
        {
            dst = dst.PyrDown().Clone();
        }
        return dst;
    }

    private int GetUpnum(Size size)
    {
        var m1 = (int)Math.Log2((1048576 / size.Width));
        var m2 = (int)Math.Log2(1048576 / size.Height);
        var m3 = (int)Math.Log2(33554432 / size.Width / size.Height);

        return new[] { m1, m2, m3 }.Min();
    }

    private void UpdateOutput(bool isDown, int num)
    {
        SendTime(() =>
        {
            var dst = _rt.NewMat();
            if (isDown)
            {
                dst = DownMat(_src, num);
            }
            else
            {
                dst = UpMat(_src, num);
            }
            _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
        });
    }

    private Mat UpMat(Mat src, int num)
    {
        var dst = _rt.T(src.Clone());
        for (int i = 0; i < num; i++)
        {
            dst = dst.PyrUp();
        }
        return dst;
    }

    #endregion PrivateFunction
}