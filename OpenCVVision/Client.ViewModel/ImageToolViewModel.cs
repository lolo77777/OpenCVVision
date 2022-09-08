using Point = System.Windows.Point;
using Rect = System.Windows.Rect;

namespace Client.ViewModel;

public class ImageToolViewModel : ViewModelBase
{
    private bool _isMouseLeftPress;

    private Point _startPoint;
    private readonly LineGeometry _line = new();
    private readonly RectangleGeometry _rect = new();
    private Point _endPoint;
    private readonly IImageDataManager _imageDataManager;

    public bool IsControlEnable { [ObservableAsProperty] get; }

    [Reactive] public Mat DisplayMat { get; set; }
    [Reactive] public bool IsDrawing { get; set; }

    [Reactive] public double TranslateTranX { get; set; }
    [Reactive] public double TranslateTranY { get; set; }
    [Reactive] public string ColorValue { get; set; }
    [Reactive] public string MatInfo { get; set; }
    [Reactive] public Geometry PathData { get; set; }
    [Reactive] public double ActualWidth { get; set; }
    [Reactive] public int GeoSelectIndex { get; set; }

    public ReactiveCommand<Point, Unit> MouseMoveCommand { get; set; }

    public ReactiveCommand<Point, Unit> MouseDownCommand { get; set; }

    public ReactiveCommand<Unit, Unit> AddGeoCommand { get; set; }
    public ReactiveCommand<Unit, Unit> ClearGeoCommand { get; set; }

    public ImageToolViewModel(IImageDataManager imageDataManager = null)
    {
        _imageDataManager = imageDataManager ?? Locator.Current.GetService<IImageDataManager>();
    }

    protected override void SetupStart()
    {
        base.SetupStart();
        PathData = _line;
    }

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        base.SetupSubscriptions(d);
        //this.WhenAnyValue(x => x.DisplayMat)
        //    .WhereNotNull()
        //    .Where(mat => !mat.Empty())
        //    .Do(mat => UpdateWriteableBitmap(ref _writeableBitmapCache, mat.Clone()))
        //    .ObserveOn(RxApp.MainThreadScheduler)
        //    .Subscribe(mat => DisplayImg = _writeableBitmapCache)
        //    .DisposeWith(d);
        this.WhenAnyValue(x => x.GeoSelectIndex)
            .Where(ind => ind >= 0)
            .Subscribe(ind => PathData = ind == 0 ? _line : _rect)
            .DisposeWith(d);
        this.WhenAnyValue(x => x.DisplayMat)
            .Select(mat => mat != null && !mat.Empty())
            .ToPropertyEx(this, x => x.IsControlEnable)
            .DisposeWith(d);
    }

    protected override void SetupCommands()
    {
        base.SetupCommands();
        MouseMoveCommand = ReactiveCommand.Create<Point, Unit>(MouseMove);
        MouseDownCommand = ReactiveCommand.Create<Point, Unit>(MouseDown);
        AddGeoCommand = ReactiveCommand.Create(AddGeometry);
        ClearGeoCommand = ReactiveCommand.Create(ClearGeometry);
    }

    private void ClearGeometry()
    {
        _line.StartPoint = new Point(-1, -1);
        _line.EndPoint = new Point(-1, -1);
        _rect.Rect = new Rect(-1, -1, 0, 0);
    }

    private void AddGeometry()
    {
        double scale = ActualWidth / DisplayMat.Width;
        int xS = (int)(_startPoint.X / scale);
        int yS = (int)(_startPoint.Y / scale);
        int xE = (int)(_endPoint.X / scale);
        int yE = (int)(_endPoint.Y / scale);
        OpenCvSharp.Point point = new(Math.Min(xS, xE), Math.Min(yS, yE));
        Size size = new(Math.Abs(xE - xS), Math.Abs(yE - yS));

        if (GeoSelectIndex == 1)
        {
            Cv2.Rectangle(DisplayMat, new OpenCvSharp.Rect(point, size), Scalar.Red);
        }
        else
        {
            Cv2.Line(DisplayMat, new OpenCvSharp.Point(xE, yE), new OpenCvSharp.Point(xS, yS), Scalar.Red);
        }

        PathData = new LineGeometry();
        _imageDataManager.AddImage("Drawing", DisplayMat);
        ClearGeometry();
    }

    #region PublicFunction

    public void Image_MouseUp(object sender, MouseButtonEventArgs e)
    {
#if DEBUG
        this.Log().Debug("触发鼠标UP");
#endif
        _isMouseLeftPress = false;
    }

    #endregion PublicFunction

    #region PrivateFunction

    private Unit MouseDown(Point point)
    {

        _isMouseLeftPress = true;
        _startPoint = new Point(point.X, point.Y);
        if (IsDrawing)
        {
            _line.StartPoint = _startPoint;
            _line.EndPoint = _startPoint;
        }

        return Unit.Default;
    }

    private Unit MouseMove(Point point)
    {
#if DEBUG
        this.Log().Debug($"X:{point.X},Y:{point.Y}");
#endif
        if (_isMouseLeftPress && IsDrawing)
        {
            _endPoint = new Point(point.X, point.Y);
            _line.EndPoint = _endPoint;
            _rect.Rect = new Rect(_startPoint, _endPoint);
        }
        else if (_isMouseLeftPress && !IsDrawing)
        {
            TranslateTranX += point.X - _startPoint.X;
            TranslateTranY += point.Y - _startPoint.Y;
        }
        MatInfo = $"{GetImgInfo(DisplayMat)}";
        (string posion, string colorValue) = GetInforMation(DisplayMat, point);

        string colorValueStr = $"{posion}    {colorValue}";
        ColorValue = colorValueStr;
        return Unit.Default;
    }

    private string GetImgInfo(Mat mat)
    {
        return $"Chanels:{mat.Channels()}, Width:{mat.Width}, Heigh:{mat.Height}, Mattype:{mat.Type().ToString()}";
    }

    private (string posion, string colorValue) GetInforMation(Mat mat, Point position)
    {
        string colorValueStr = string.Empty;
        double scale = ActualWidth / mat.Width;
        double x = position.X / scale;
        double y = position.Y / scale;
        string posionStr = $"X:{x:F3},Y:{y:F3}";
        if (y < mat.Height && x < mat.Width)
        {
            if (mat.Channels() == 3)
            {
                Mat.UnsafeIndexer<Vec3b> matInd = mat.GetUnsafeGenericIndexer<Vec3b>();
                Vec3b vec3 = matInd[(int)y, (int)x];
                colorValueStr = $"B:{vec3.Item0};G:{vec3.Item1};R:{vec3.Item2}";
            }
            else if (mat.Channels() == 1)
            {
                Mat.UnsafeIndexer<byte> matInd = mat.GetUnsafeGenericIndexer<byte>();
                byte value = matInd[(int)y, (int)x];
                colorValueStr = $"Gray:{value}";
            }
        }

        return (posionStr, colorValueStr);
    }

    #endregion PrivateFunction
}