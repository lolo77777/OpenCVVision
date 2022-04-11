namespace Client.ViewModel.Operation;

[OperationInfo(6.1, "HoughCircle", "Circle")]
public class CircleDetectViewModel : OperaViewModelBase
{
    private CircleSegment[] _circleSegments;

    [Reactive] public double Dp { get; set; } = 2;
    [Reactive] public double MinDist { get; set; } = 10;
    [Reactive] public int MinRadius { get; set; } = 5;
    [Reactive] public int MaxRadius { get; set; } = 500;
    [Reactive] public double CannyParam { get; set; } = 100;
    [Reactive] public double Param2 { get; set; } = 100;
    [Reactive] public int SelectCircleValue { get; set; }

    [ObservableAsProperty] public IEnumerable<int> CircleItems { get; }

    protected override void SetupDeactivate()
    {
        base.SetupDeactivate();
    }

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        base.SetupSubscriptions(d);
        _imageDataManager.InputMatGuidSubject
            .Select(guid => _imageDataManager.GetCurrentMat())
            .WhereNotNull()
            .Where(mat => CanOperat)
            .Select(vt => Updateoutput())
            .WhereNotNull()
            .Where(its => its.Any())
            .Select(its =>
            {
                return Enumerable.Range(-1, its.Count() + 1);
            })
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToPropertyEx(this, x => x.CircleItems)
            .DisposeWith(d);

        this.WhenAnyValue(x => x.Dp, x => x.MinDist, x => x.MinRadius, x => x.MaxRadius, x => x.CannyParam, x => x.Param2)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .Select(vt => Updateoutput())
            .WhereNotNull()
            .Where(its => its.Any())
            .Select(its =>
            {
                var a = Enumerable.Range(-1, its.Count() + 1);
                var b = _circleSegments;
                return a;
            })
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToPropertyEx(this, x => x.CircleItems)
            .DisposeWith(d);
        //this.WhenAnyValue(x => x.CircleItems)
        //    .WhereNotNull()
        //    .Where(its => its.Count() > 1)
        //    .Select(its => its.Count())
        //    .ObserveOn(RxApp.MainThreadScheduler)
        //    .BindTo(this, x => x.SelectIndexCircle)
        //    .DisposeWith(d);
        this.WhenAnyValue(x => x.SelectCircleValue)
            .Where(i => i >= -1 && CircleItems != null && CircleItems.Any())
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(i => UpdateOutputImage(i))
            .DisposeWith(d);
    }

    private CircleSegment[] Updateoutput()
    {
        SendTime(() =>
        {
            _circleSegments = Cv2.HoughCircles(_sigleSrc, HoughModes.Gradient, Dp, MinDist, CannyParam
                    , Param2, MinRadius, MaxRadius);
        }, true);

        return _circleSegments;
    }

    private void UpdateOutputImage(int selectIndex)
    {
        SendTime(() =>
        {
            Mat dst = _rt.T(_sigleSrc.Clone().CvtColor(ColorConversionCodes.GRAY2BGR));
            if (selectIndex == -1)
            {
                for (int i = 0; i < _circleSegments.Length; i++)
                {
                    Cv2.Circle(dst, _circleSegments[i].Center.ToPoint(), (int)_circleSegments[i].Radius, Scalar.RandomColor(), 10);
                }
            }
            else
            {
                Cv2.Circle(dst, _circleSegments[selectIndex].Center.ToPoint(), (int)_circleSegments[selectIndex].Radius, Scalar.RandomColor(), 10);
            }
            _imageDataManager.OutputMatSubject.OnNext(dst.Clone());
        });
    }
}