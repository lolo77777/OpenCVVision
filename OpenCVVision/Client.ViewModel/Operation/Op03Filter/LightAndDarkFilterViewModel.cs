namespace Client.ViewModel.Operation;

public enum LightAndDarkEnum
{
    暗,
    亮,
    亮暗
}

[OperationInfo(3.5, "明暗筛选", "ThemeLightDark")]
public class LightAndDarkFilterViewModel : OperaViewModelBase
{
    [Reactive] public int KernelSize { get; set; } = 16;
    [Reactive] public int SizeWidth { get; set; } = 1;
    [ObservableAsProperty] public int SizeHeight { get; }
    [Reactive] public int TypeSelectIndexed { get; set; }
    [Reactive] public float LightScale { get; set; } = 50;
    [Reactive] public float DarkScale { get; set; } = 50;
    public ObservableCollection<string> LightAndDarkType { get; set; }

    protected override void SetupStart()
    {
        base.SetupStart();
        LightAndDarkType = new ObservableCollection<string>(Enum.GetNames(typeof(LightAndDarkEnum)));
    }

    private void UpdateUi(int kernelSize, int sizeW, int type, float lightScale, float darkScale)
    {
        SendTime(() =>
        {
            lightScale = lightScale / 100f;
            darkScale = darkScale / 100f;
            var dst = _rt.T(_sigleSrc.Clone());
            var ind = dst.GetGenericIndexer<byte>();
            for (int y = 0; y < _src.Height; y += kernelSize / sizeW)
            {
                for (int x = 0; x < _src.Width; x += sizeW)
                {
                    if (x + sizeW <= _src.Width && y + (kernelSize / sizeW) <= _src.Height)
                    {
                        byte[] btsTmp = new byte[kernelSize];
                        for (int j = 0; j < (kernelSize / sizeW); j++)
                        {
                            for (int k = 0; k < sizeW; k++)
                            {
                                btsTmp[j * sizeW + k] = ind[y + j, x + k];
                            }
                        }
                        btsTmp.AsSpan().Sort();
                        var midValue = btsTmp[btsTmp.Length / 2];
                        var maxVal = midValue + (255 - midValue) * lightScale;
                        var minVal = midValue - midValue * darkScale;
                        for (int j = 0; j < kernelSize / sizeW; j++)
                        {
                            for (int k = 0; k < sizeW; k++)
                            {
                                switch (type)
                                {
                                    case 0:
                                        //保留特别暗的
                                        if (ind[y + j, x + k] >= minVal)
                                        {
                                            ind[y + j, x + k] = 0;
                                        }
                                        else
                                        {
                                            ind[y + j, x + k] = 255;
                                        }
                                        break;

                                    case 1:
                                        //保留特别亮的
                                        if (ind[y + j, x + k] <= maxVal)
                                        {
                                            ind[y + j, x + k] = 0;
                                        }
                                        else
                                        {
                                            ind[y + j, x + k] = 255;
                                        }
                                        break;

                                    case 2:
                                        //明暗独立
                                        if (ind[y + j, x + k] >= minVal && ind[y + j, x + k] <= maxVal)
                                        {
                                            ind[y + j, x + k] = 0;
                                        }
                                        else
                                        {
                                            ind[y + j, x + k] = 255;
                                        }
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
        });
    }

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        base.SetupSubscriptions(d);
        this.WhenAnyValue(x => x.KernelSize, x => x.SizeWidth, x => x.TypeSelectIndexed, x => x.LightScale, x => x.DarkScale)
            .Throttle(TimeSpan.FromMilliseconds(400))
            .Where(vt => vt.Item1 >= 16 && vt.Item2 >= 1 && vt.Item3 >= 0 && vt.Item4 > 0 && vt.Item5 > 0)
            .Subscribe(_ => UpdateUi(KernelSize, SizeWidth, TypeSelectIndexed, LightScale, DarkScale))
            .DisposeWith(d);
        _imageDataManager.InputMatGuidSubject
            .WhereNotNull()
            .Where(guid => CanOperat)
            .Where(_ => KernelSize >= 16 && SizeWidth >= 1 && TypeSelectIndexed >= 0 && LightScale > 0 && DarkScale > 0)
            .Subscribe(guid => UpdateUi(KernelSize, SizeWidth, TypeSelectIndexed, LightScale, DarkScale))
            .DisposeWith(d);
        this.WhenAnyValue(x => x.KernelSize, x => x.SizeWidth)
            .Throttle(TimeSpan.FromMilliseconds(400))
            .Where(vt => vt.Item1 >= 16 && vt.Item2 >= 1)
            .Select(vt => vt.Item1 / vt.Item2)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToPropertyEx(this, x => x.SizeHeight)
            .DisposeWith(d);
    }
}