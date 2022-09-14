namespace OpenCVVision.ViewModel.Operation;

[OperationInfo(4.2, "两图像计算", "ImageMultiple")]
public class TwoMatOperationViewModel : OperaViewModelBase
{
    private ReadOnlyObservableCollection<string> _imageItems;
    public ReadOnlyObservableCollection<string> ImageItems => _imageItems;
    public IList<string> OperaMethodItems { get; private set; }
    [Reactive] public string FirstImageSelectValue { get; private set; }
    [Reactive] public string SecondImageSelectValue { get; private set; }
    [Reactive] public string OperaMethod { get; private set; }
    public bool IsOperaEnable { [ObservableAsProperty] get; }

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        base.SetupSubscriptions(d);
        _imageDataManager.SourceCacheImageData
            .Connect()
            .Transform(t => t.TxtMarker)
            .Where(vs => vs.Count >= 2)
            .Bind(out _imageItems)
            .Subscribe()
            .DisposeWith(d);
        this.WhenAnyValue(x => x.FirstImageSelectValue, x => x.SecondImageSelectValue)
            .Where(vt => vt.Item1 != null && vt.Item2 != null)
            .Select(vt =>
            {
                Mat mat1 = _rt.T(_imageDataManager.GetImage(vt.Item1).ImageMat?.Clone());
                Mat mat2 = _rt.T(_imageDataManager.GetImage(vt.Item2).ImageMat?.Clone());
                return mat1 != null && mat2 != null && mat1.Size() == mat2.Size() && mat1.Type() == mat2.Type();
            })
            .ToPropertyEx(this, x => x.IsOperaEnable)
            .DisposeWith(d);
        _imageDataManager.InputMatGuidSubject
            .Where(guid => CanOperat && !string.IsNullOrWhiteSpace(OperaMethod))
            .Subscribe(guid => UpdateOutput())
            .DisposeWith(d);
        this.WhenAnyValue(x => x.OperaMethod)
            .Where(str => !string.IsNullOrWhiteSpace(str) && CanOperat && IsOperaEnable)
            .Subscribe(str => UpdateOutput())
            .DisposeWith(d);
    }

    protected override void SetupStart()
    {
        base.SetupStart();
        OperaMethodItems = new[] { "Add", "Subtract", "Max", "Min", "BitwiseAnd", "BitwiseOr", "BitwiseXOr" };
    }

    private void UpdateOutput()
    {
        SendTime(() =>
        {
            Mat reMat = _rt.NewMat();

            Mat dst1 = _rt.T(_imageDataManager.GetImage(FirstImageSelectValue).ImageMat?.Clone());
            Mat dst2 = _rt.T(_imageDataManager.GetImage(SecondImageSelectValue).ImageMat?.Clone());
            if (dst1 != null && dst2 != null)
            {
                switch (OperaMethod)
                {
                    case "Add":
                        reMat = dst1 + dst2;
                        break;

                    case "Subtract":
                        reMat = dst1 - dst2;
                        break;

                    case "Max":
                        Cv2.Max(dst1, dst2, reMat);
                        break;

                    case "Min":
                        Cv2.Min(dst1, dst2, reMat);
                        break;

                    case "BitwiseAnd":
                        Cv2.BitwiseAnd(dst1, dst2, reMat);
                        break;

                    case "BitwiseOr":
                        Cv2.BitwiseOr(dst1, dst2, reMat);
                        break;

                    case "BitwiseXOr":
                        Cv2.BitwiseXor(dst1, dst2, reMat);
                        break;

                    default:
                        break;
                }
                _imageDataManager.OutputMatSubject.OnNext(reMat.Clone());
            }
        });
    }
}