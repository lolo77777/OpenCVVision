namespace SprinklerDetect.Services.HardWare.Camera;

public class DaHengCamera : ICamera
{
    private readonly List<string> _deviceListStr = new();
    private IGXDevice? _objIGXDevice;
    private GX_DEVICE_OFFLINE_CALLBACK_HANDLE? _hDeviceOffline;
    private IGXStream? _objIGXStream;
    private IGXFeatureControl? _objIGXStreamFeatureControl;
    private IGXFeatureControl? _objIGXFeatureControl;
    private Mat? _matGrab;

    public ReplaySubject<(string, LogLevel)> StatusMsg { get; } = new();
    public Subject<Mat> GrabMatSubject { get; } = new();
    public ReadOnlyCollection<string> DeviceListStr { get; }
    public BehaviorSubject<bool> ConnectStatus { get; } = new(false);
    public BehaviorSubject<bool> IsGrabing { get; } = new(false);

    public DaHengCamera()
    {
        DeviceListStr = _deviceListStr.AsReadOnly();
        TryCatchGX(() => IGXFactory.GetInstance().Init());
    }

    public Result CloseDevices()
    {
        return TryCatchGx(() =>
        {
            //流对象控制或者采集
            //当用户不使用流对象的时候，需要将其关闭
            _objIGXStream?.Close();
            _objIGXStream?.Close();

            _objIGXDevice?.UnregisterDeviceOfflineCallback(_hDeviceOffline);
            _objIGXDevice?.Close();
            ConnectStatus.OnNext(false);
        });
    }

    public Result ConnectDevices(int selectIndex = 0)
    {
        return TryCatchGx(() =>
        {
            if (!ConnectStatus.Value && DeviceListStr.Count > 0 && selectIndex >= 0)
            {
                _objIGXDevice = IGXFactory.GetInstance().OpenDeviceBySN(_deviceListStr[selectIndex], GX_ACCESS_MODE.GX_ACCESS_EXCLUSIVE);

                //objIGXDevice 为 IGXDevice 设备对象，设备已经打开
                //第一个参数可以传入一个字符串，例如设备名称，此参数为用户私有参数，用户可以在回调
                //函数内部将其还原然后打印，如果不需要则可传入 null 即可
                _hDeviceOffline = _objIGXDevice.RegisterDeviceOfflineCallback(null, OnDeviceOfflineEvent);
                _objIGXFeatureControl = _objIGXDevice?.GetRemoteFeatureControl();

                //打开流
                var nStreamNum = _objIGXDevice?.GetStreamCount();
                if (nStreamNum > 0)
                {
                    _objIGXStream = _objIGXDevice?.OpenStream(0);
                    _objIGXStreamFeatureControl = _objIGXStream?.GetFeatureControl();
                }
                //以提高网络相机的采集性能,设置方法参考以下代码（目前只有千兆网系列相机支持设置最优包长）
                var objDeviceClass = _objIGXDevice?.GetDeviceInfo().GetDeviceClass();
                if (GX_DEVICE_CLASS_LIST.GX_DEVICE_CLASS_GEV == objDeviceClass)
                {
                    //判断设备是否支持流通道数据包功能
                    if (_objIGXFeatureControl?.IsImplemented("GevSCPSPacketSize") == true)
                    {
                        //获取当前网络环境的最优包长值
                        var nPacketSize = _objIGXStream?.GetOptimalPacketSize();
                        //将最优包长值设置为当前设备的流通道包长值
                        if (nPacketSize.HasValue)
                        {
                            _objIGXFeatureControl.GetIntFeature("GevSCPSPacketSize").SetValue(nPacketSize.Value);
                        }
                    }
                }
                ConnectStatus.OnNext(true);
            }
        });
    }

    private void OnDeviceOfflineEvent(object objUserParam)
    {
        StatusMsg.OnNext(("设备已掉线", LogLevel.Error));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _matGrab?.Dispose();
            _objIGXDevice = null;
            _objIGXFeatureControl = null;
            _objIGXStream = null;
            _objIGXStreamFeatureControl = null;
            IGXFactory.GetInstance().Uninit();
        }
    }

    public Task<Result> FreeGrab(int milsec)
    {
        throw new NotImplementedException();
    }

    public Result SearchDevices()
    {
        return TryCatchGx(() =>
        {
            _deviceListStr.Clear();
            List<IGXDeviceInfo> iGXDeviceInfos = new(4);
            IGXFactory.GetInstance().UpdateAllDeviceList(200, iGXDeviceInfos);
            if (iGXDeviceInfos.Count > 0)
            {
                iGXDeviceInfos.ForEach(info => _deviceListStr.Add(info.GetSN()));
            }
        });
    }

    public Result SetupDevices()
    {
        return TryCatchGx(() =>
        {
            //设置采集模式连续采集
            _objIGXFeatureControl?.GetEnumFeature("AcquisitionMode").SetValue("Continuous");

            //设置触发模式为开
            _objIGXFeatureControl?.GetEnumFeature("TriggerMode").SetValue("Off");

            //选择触发源为软触发
            _objIGXFeatureControl?.GetEnumFeature("TriggerSource").SetValue("Software");

            _objIGXFeatureControl?.GetIntFeature("Width").SetValue(2592);
            _objIGXFeatureControl?.GetIntFeature("Height").SetValue(1944);
            _objIGXFeatureControl?.GetEnumFeature("ExposureAuto").SetValue("Continuous");
            //_objIGXFeatureControl?.GetFloatFeature("ExposureTime").SetValue(90000);
            _objIGXFeatureControl?.GetEnumFeature("BalanceWhiteAuto").SetValue("Continuous");
            _objIGXFeatureControl?.GetIntFeature("Saturation").SetValue(80);
            //颜色增益
            _objIGXFeatureControl?.GetEnumFeature("GainSelector").SetValue("AnalogAll");
            _objIGXFeatureControl?.GetEnumFeature("GainAuto").SetValue("Continuous");
            _objIGXFeatureControl?.GetBoolFeature("GammaEnable").SetValue(true);
        });
    }

    public Result StartGrab()
    {
        return TryCatchGx(() =>
        {
            if (ConnectStatus.Value)
            {
                _objIGXStreamFeatureControl?.GetEnumFeature("StreamBufferHandlingMode").SetValue("OldestFirst");
                //注册采集回调函数，注意第一个参数是用户私有参数，用户可以传入任何 object 对象，也可以是 null
                //用户私有参数在回调函数内部还原使用，如果不使用私有参数，可以传入 null
                _objIGXStream?.RegisterCaptureCallback(_objIGXDevice, OnFrameCallbackFun);
                //开启流通道采集
                _objIGXStream?.StartGrab();
                //给设备发送开采命令
                _objIGXFeatureControl?.GetCommandFeature("AcquisitionStart").Execute();
                IsGrabing.OnNext(true);
            }
        });
    }

    private void OnFrameCallbackFun(object objUserParam, IFrameData objIFrameData)
    {
        //用户私有参数 obj，用户在注册回调函数的时候传入了设备对象，在回调函数内部可以将此
        //参数还原为用户私有参数

        if (objIFrameData.GetStatus() == GX_FRAME_STATUS_LIST.GX_FRAME_STATUS_SUCCESS)
        {
            //图像获取为完整帧，可以读取图像宽、高、数据格式等
            var nWidth = objIFrameData.GetWidth();
            var nHeight = objIFrameData.GetHeight();

            //其他图像信息的获取参见 IFrameData 接口定义
            var emPixelFormat = objIFrameData.GetPixelFormat();
            switch (emPixelFormat)
            {
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BGR16:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BGR14:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BGR12:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BGR10:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BGR8:
                    _matGrab = new Mat((int)nHeight, (int)nWidth, MatType.CV_8UC3, objIFrameData.GetBuffer());
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_BG12:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GB12:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_RG12:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GR12:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_BG10:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GB10:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_RG10:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GR10:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_BG8:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GB8:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_RG8:
                    var pRGB24Buffer = objIFrameData.ConvertToRGB24(GX_VALID_BIT_LIST.GX_BIT_0_7, GX_BAYER_CONVERT_TYPE_LIST.GX_RAW2RGB_NEIGHBOUR, true);
                    _matGrab = new Mat((int)nHeight, (int)nWidth, MatType.CV_8UC3, pRGB24Buffer);
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GR8:
                    var pRGB24Buffer1 = objIFrameData.ConvertToRGB24(GX_VALID_BIT_LIST.GX_BIT_0_7, GX_BAYER_CONVERT_TYPE_LIST.GX_RAW2RGB_NEIGHBOUR, false);
                    _matGrab = new Mat((int)nHeight, (int)nWidth, MatType.CV_8UC3, pRGB24Buffer1);
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO16:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO14:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO12:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO10:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO8_SIGNED:
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO8:
                    _matGrab = new Mat((int)nHeight, (int)nWidth, MatType.CV_8UC1, objIFrameData.GetBuffer());
                    break;

                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_UNDEFINED:
                    break;

                default:
                    break;
            }
            if (_matGrab != null)
            {
                GrabMatSubject.OnNext(_matGrab.Clone());
                _matGrab.Dispose();
            }
        }
    }

    public Result StopGrab()
    {
        return TryCatchGx(() =>
        {
            IsGrabing.OnNext(false);
            //停采、注销采集回调函数
            _objIGXFeatureControl?.GetCommandFeature("AcquisitionStop").Execute();
            _objIGXStream?.StopGrab();
            _objIGXStream?.UnregisterCaptureCallback();
        });
    }

    private void TryCatchGX(Action action)
    {
        try
        {
            action.Invoke();
        }
        catch (CGalaxyException ex)
        {
            StatusMsg.OnNext(($"错误码：{ex.GetErrorCode()},错误信息：{ex.Message}", LogLevel.Error));
        }
    }

    private Result TryCatchGx(Action action)
    {
        try
        {
            action.Invoke();
            return Result.Ok();
        }
        catch (CGalaxyException ex)
        {
            var msg = $"错误码：{ex.GetErrorCode()},错误信息：{ex.Message}";
            StatusMsg.OnNext((msg, LogLevel.Error));
            return Result.Fail(msg);
        }
    }
}