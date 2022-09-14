namespace OpenCVVision.ViewModel.Operation;

[OperationInfo(6.3, "Blob", "BloodBag")]
public class BlobDetectViewModel : OperaViewModelBase
{
    private SimpleBlobDetector.Params _params = new();
    private SimpleBlobDetector _simpleBlobDetector;
    [Reactive] public bool IsEnableColor { get; set; }
    [Reactive] public byte BlobColorSam { get; set; } = 255;
    [Reactive] public bool IsEnableArea { get; set; }
    [Reactive] public bool IsEnableCircularity { get; set; }
    [Reactive] public bool IsEnableConvexity { get; set; }
    [Reactive] public bool IsEnableInertia { get; set; }
    [Reactive] public float MinAreaSam { get; set; } = 10;
    [Reactive] public float MaxAreaSam { get; set; } = 200;
    [Reactive] public float MinCircularitySam { get; set; } = 0.1f;
    [Reactive] public float MaxCircularitySam { get; set; } = 0.9f;
    [Reactive] public float MaxConvexitySam { get; set; } = 0.9f;
    [Reactive] public float MinConvexitySam { get; set; } = 0.1f;
    [Reactive] public float MaxInertiaRatioSam { get; set; } = 0.9f;
    [Reactive] public float MinInertiaRatioSam { get; set; } = 0.1f;
    [Reactive] public float MinDistBetweenBlobsSam { get; set; } = 0f;
    [Reactive] public float ThresholdStepSam { get; set; } = 10f;
    [Reactive] public float MinThresholdSam { get; set; } = 10f;
    [Reactive] public float MaxThresholdSam { get; set; } = 200f;

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        base.SetupSubscriptions(d);
        var areaOb = this.WhenAnyValue(x => x.IsEnableArea, x => x.MinAreaSam, x => x.MaxAreaSam)
            .Where(vt => vt.Item3 > vt.Item2);
        var colorOb = this.WhenAnyValue(x => x.IsEnableColor, x => x.BlobColorSam)
            .Where(vt => vt.Item2 >= 0);
        var cirOb = this.WhenAnyValue(x => x.IsEnableCircularity, x => x.MinCircularitySam, x => x.MaxCircularitySam)
            .Where(vt => vt.Item3 > vt.Item2);
        var conOb = this.WhenAnyValue(x => x.IsEnableConvexity, x => x.MinConvexitySam, x => x.MaxConvexitySam)
            .Where(vt => vt.Item3 > vt.Item2);
        var inertiaOb = this.WhenAnyValue(x => x.IsEnableInertia, x => x.MinInertiaRatioSam, x => x.MaxInertiaRatioSam)
            .Where(vt => vt.Item3 > vt.Item2);
        var thresholdOb = this.WhenAnyValue(x => x.MinDistBetweenBlobsSam, x => x.MinThresholdSam, x => x.MaxThresholdSam, x => x.ThresholdStepSam)
            .Where(vt => vt.Item1 >= 0 && vt.Item3 > vt.Item2 && vt.Item4 > 0);
        var ob1 = areaOb.Merge(cirOb).Merge(conOb).Merge(inertiaOb);
        ob1
            .Throttle(TimeSpan.FromMilliseconds(300))
            .Subscribe(vt => UpdateOutput())
            .DisposeWith(d);
        colorOb
            .Throttle(TimeSpan.FromMilliseconds(300))
            .Subscribe(vt => UpdateOutput())
            .DisposeWith(d);
        thresholdOb
            .Throttle(TimeSpan.FromMilliseconds(300))
            .Subscribe(vt => UpdateOutput());
        _imageDataManager.InputMatGuidSubject
           .Select(guid => _imageDataManager.GetCurrentMat())
           .WhereNotNull()
           .Where(mat => CanOperat)
           .Subscribe(vt => UpdateOutput())
           .DisposeWith(d);
    }

    protected override void SetupStart()
    {
        base.SetupStart();
    }

    private void UpdateOutput()
    {
        SendTime(() =>
        {
            Mat dst = _rt.T(_src.Clone());
            _params.MinDistBetweenBlobs = MinDistBetweenBlobsSam;
            _params.ThresholdStep = ThresholdStepSam;
            _params.MinThreshold = MinThresholdSam;
            _params.MaxThreshold = MaxThresholdSam;

            _params.FilterByColor = IsEnableColor;
            _params.BlobColor = BlobColorSam;
            _params.FilterByArea = IsEnableArea;
            _params.MinArea = MinAreaSam;
            _params.MaxArea = MaxAreaSam;
            _params.FilterByCircularity = IsEnableCircularity;
            _params.MinCircularity = MinCircularitySam;
            _params.MaxCircularity = MaxCircularitySam;
            _params.FilterByConvexity = IsEnableConvexity;
            _params.MinConvexity = MinConvexitySam;
            _params.MaxConvexity = MaxConvexitySam;
            _params.FilterByInertia = IsEnableInertia;
            _params.MinInertiaRatio = MinInertiaRatioSam;
            _params.MaxInertiaRatio = MaxInertiaRatioSam;
            _simpleBlobDetector = SimpleBlobDetector.Create(_params);
            var keypoints = _simpleBlobDetector.Detect(_src);

            if (dst.Channels() == 1)
            {
                Cv2.CvtColor(dst, dst, ColorConversionCodes.GRAY2BGR);
            }
            Cv2.DrawKeypoints(dst, keypoints, dst, Scalar.DarkRed, DrawMatchesFlags.DrawRichKeypoints);
            _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
        }, true);
    }
}