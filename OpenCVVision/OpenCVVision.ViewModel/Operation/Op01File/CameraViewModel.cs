namespace OpenCVVision.ViewModel.Operation;

[OperationInfo(1.2, "相机图片", "Camera")]
public class CameraViewModel : OperaViewModelBase
{
    private ICamera _camera;
    public IEnumerable<string> CameraFactorys => new[] { "海康", "大恒" };
    [Reactive] public ICamera Camera { get; set; } = null;
    [Reactive] public bool IsConnectCamera { get; set; }
    [Reactive] public bool IsStartGrab { get; set; }
    [Reactive] public ReadOnlyCollection<string> CameraInfoList { get; set; }
    [Reactive] public string CameraFactory { get; set; } = string.Empty;
    [Reactive] public int CameraSelectIndex { get; set; }
    [ObservableAsProperty] public bool CanCamOpera { get; }
    public ICommand SearchCommand { get; set; }
    public ICommand ConnectCommand { get; set; }
    public ICommand StartGrabCommand { get; set; }
    public ICommand PauseGrabCommand { get; set; }
    public ICommand CloseCommand { get; set; }

    protected override void SetupCommands()
    {
        SearchCommand = ReactiveCommand.Create(SearchCamera);
        var canConnect = this.WhenAnyValue(x => x.CameraInfoList, x => x.CameraSelectIndex)
                            .Select(vt => vt.Item1?.Count > 0 && vt.Item2 >= 0 && _camera != null);
        ConnectCommand = ReactiveCommand.Create(ConnectCammera, canConnect);
        var canStart = _camera?.ConnectStatus.Select(b => b && _camera?.IsGrabing.Value == false)
                    .Merge(_camera?.IsGrabing.Select(b => !b && _camera?.ConnectStatus.Value == true));
        StartGrabCommand = ReactiveCommand.Create(StartGrab, canStart);
        var canPause = _camera?.ConnectStatus.Select(b => b && _camera?.IsGrabing.Value == true)
                    .Merge(_camera?.IsGrabing.Select(b => b && _camera?.ConnectStatus.Value == true));

        PauseGrabCommand = ReactiveCommand.Create(PauseGrab, canPause);
        CloseCommand = ReactiveCommand.Create(CloseCamera, _camera?.ConnectStatus);
    }

    private void PauseGrab()
    {
        var ret = _camera.StopGrab();
        if (ret.IsSuccess)
        {
            this.Log().Info("采集已暂停");
        }
        else
        {
            this.Log().Error($"暂停失败:{ret.Errors[0].Message}");
        }
    }

    private void CloseCamera()
    {
        var ret = _camera.CloseDevices();
        if (ret.IsSuccess)
        {
            this.Log().Info("相机已关闭");
        }
        else
        {
            this.Log().Error($"相机关闭失败:{ret.Errors[0].Message}");
        }
    }

    private void StartGrab()
    {
        var ret = _camera.StartGrab();
        if (ret.IsSuccess)
        {
            this.Log().Info("采集已开始");
        }
        else
        {
            this.Log().Error($"相机开始失败:{ret.Errors[0].Message}");
        }
    }

    private void ConnectCammera()
    {
        var ret = _camera.ConnectDevices(CameraSelectIndex);
        if (ret.IsSuccess)
        {
            this.Log().Info("相机连接成功");
        }
        else
        {
            this.Log().Error($"相机连接失败:{ret.Errors[0].Message}");
        }
    }

    private void SearchCamera()
    {
        if (!string.IsNullOrEmpty(CameraFactory))
        {
            _camera = Locator.Current.GetService<ICamera>(CameraFactory);
            Camera = _camera;
            if (_camera != null)
            {
                var ret = _camera.SearchDevices();
                if (ret.IsSuccess)
                {
                    CameraInfoList = _camera.DeviceListStr;
                    this.Log().Info($"搜索相机完毕，发现{_camera.DeviceListStr.Count}台相机");
                }
                else
                {
                    this.Log().Error($"相机搜索失败:{ret.Errors[0].Message}");
                }
            }
        }
    }

    protected override void SetupDeactivate()
    {
        if (_camera != null)
        {
            if (_camera?.ConnectStatus.Value == true)
            {
                _camera.CloseDevices();
            }

            _camera.Dispose();
            _camera = null;
        }

        base.SetupDeactivate();
    }

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        _camera?.GrabMatSubject
            .Where(mat => mat?.Empty() == false)
            .Subscribe(mat =>
            {
                _imageDataManager.OutputMatSubject.OnNext(mat.Clone());
                mat.Dispose();
            }).DisposeWith(d);
        this.WhenAnyValue(x => x.Camera, x => x.Camera.ConnectStatus)
            .Select(vt => vt.Item1 != null && vt.Item2.Value)
            .ToPropertyEx(this, x => x.CanCamOpera)
            .DisposeWith(d);
        base.SetupSubscriptions(d);
    }
}