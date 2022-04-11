namespace Client.ViewModel.Operation;

[OperationInfo(8.1, "线激光中心", "LaserPointer")]
public class LaserLineViewModel : OperaViewModelBase
{
    private ResourcesTracker _resourcesTracker = new();
    private LightPlaneCal _lightPlaneCal;
    private LightPlaneCalibrate _lightPlaneCalibrate;
    public ReactiveCommand<Unit, Unit> CalibrateTestCommand { get; set; }
    public ReactiveCommand<Unit, Unit> LaserLigthCalCommand { get; set; }

    protected override void SetupStart()
    {
        base.SetupStart();
        _lightPlaneCalibrate = _resolver.GetService<LightPlaneCalibrate>();
        _lightPlaneCal = _resolver.GetService<LightPlaneCal>();
    }

    protected override void SetupCommands()
    {
        base.SetupCommands();
        CalibrateTestCommand = ReactiveCommand.Create(CalibrateTest);
        LaserLigthCalCommand = ReactiveCommand.Create(CalTest);
    }

    protected override void SetupDeactivate()
    {
        base.SetupDeactivate();
        _resourcesTracker.Dispose();
        _lightPlaneCal.Dispose();
        _lightPlaneCalibrate.Dispose();
    }

    #region PrivateFunction

    private void CalibrateTest()
    {
        _lightPlaneCalibrate.Calibrate();
    }

    private void CalTest()
    {
        SendTime(() =>
        {
            using FileStorage fr = new(FilePath.File.LaserLineCaliYaml, FileStorage.Modes.Read);
            Mat _cameraMatrixMat = _resourcesTracker.T(fr["CameraMatrixMat"].ReadMat());
            Mat _lightPlaneCoeffient = _resourcesTracker.T(fr["LightPlaneCoeffient"].ReadMat());
            Mat _cameraToLightPlaneMat = _resourcesTracker.T(fr["CameraToLightPlaneMat"].ReadMat());
            Mat _templete = _resourcesTracker.T(Cv2.ImRead(FilePath.Image.LaserLineLightTemplate, ImreadModes.Grayscale).PyrDown().PyrDown().PyrDown().PyrDown().GaussianBlur(new Size(3, 3), 0.24));
            IList<Mat> files = Directory.GetFiles(FilePath.Folder.LaserLineTestFolder).Select(str => _resourcesTracker.T(Cv2.ImRead(str, ImreadModes.Grayscale))).ToList();
            int num = 0;
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(1000))
                .Take(files.Count())
                .Select(i => files.ElementAt(num))
                .Do(mat =>
                {
                    num++;
                    (Point2d[] pt2fs, Mat thinMat) = _lightPlaneCal.GetResultGray(mat, _templete, _cameraToLightPlaneMat, _cameraMatrixMat, _lightPlaneCoeffient);
                    _imageDataManager.OutputMatSubject.OnNext(thinMat);
                })
                .Subscribe();
        });
    }

    #endregion PrivateFunction
}