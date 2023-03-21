using MvCamCtrl.NET.CameraParams;

using System.Diagnostics;

namespace OpenCVVision.Services.Camera;

public class HIKCamera : ICamera
{
    private CCamera _myCamera;
    private int _nRet;
    private bool _disposedValue;
    private readonly cbExceptiondelegate _ExceptionCallBack;
    private readonly cbOutputExdelegate _imageCallBack;
    private int _selectDevice;
    private Mat _grabMatTemp = new();
    private List<CCameraInfo> m_ltDeviceList = new();
    private readonly List<string> _deviceListStr = new();
    public ReadOnlyCollection<string> DeviceListStr { get; }
    public ReplaySubject<(string, LogLevel)> StatusMsg { get; } = new(100, TimeSpan.FromMilliseconds(60));
    public Subject<Mat> GrabMatSubject { get; } = new();
    public BehaviorSubject<bool> ConnectStatus { get; } = new(false);
    public BehaviorSubject<bool> IsGrabing { get; } = new(false);

    public HIKCamera()
    {
        _myCamera = new CCamera();
        _imageCallBack = new cbOutputExdelegate(OnGrab);
        _ExceptionCallBack = new cbExceptiondelegate(OnException);
        DeviceListStr = new ReadOnlyCollection<string>(_deviceListStr);
    }

    public Result CloseDevices()
    {
        if (ConnectStatus.Value)
        {
            _nRet = _myCamera.CloseDevice();
            if (_nRet != CErrorDefine.MV_OK)
            {
                StatusMsg.OnNext(($"关闭设备失败，返回{_nRet:X8}", LogLevel.Error));
                return Result.Fail($"关闭设备失败，返回{_nRet:X8}");
            }
        }

        ConnectStatus.OnNext(false);
        IsGrabing.OnNext(false);
        return Result.Ok();
    }

    public Result ConnectDevices(int selectIndex = 0)
    {
        if (m_ltDeviceList.Count > 0 && DeviceListStr.Any() && selectIndex != -1)
        {
            _selectDevice = selectIndex;

            var device = m_ltDeviceList[_selectDevice];
            _myCamera = new CCamera();
            _nRet = _myCamera.CreateHandle(ref device);
            if (_nRet != CErrorDefine.MV_OK)
            {
                ConnectStatus.OnNext(false);
                return Result.Fail("创建相机设备类型失败！");
            }
            _nRet = _myCamera.OpenDevice();
            if (_nRet != CErrorDefine.MV_OK)
            {
                _myCamera.DestroyHandle();

                StatusMsg.OnNext(($"设备打开失败!MV_CC_OpenDevice_NET返回:{_nRet:X8}", LogLevel.Error));
                ConnectStatus.OnNext(false);
                return Result.Fail($"设备打开失败!MV_CC_OpenDevice_NET返回:{_nRet:X8}");
            }
            // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only
            // works for the GigE camera)
            if (device.nTLayerType == CSystem.MV_GIGE_DEVICE)
            {
                int nPacketSize = _myCamera.GIGE_GetOptimalPacketSize();
                if (nPacketSize > 0)
                {
                    _nRet = _myCamera.SetIntValue("GevSCPSPacketSize", (uint)nPacketSize);
                    if (_nRet != CErrorDefine.MV_OK)
                    {
                        StatusMsg.OnNext(($"Set Packet Size failed! MV_CC_SetIntValue_NET返回:{_nRet:X8}", LogLevel.Error));
                    }
                }
                else
                {
                    StatusMsg.OnNext(($"Get Packet Size failed! MV_CC_GetOptimalPacketSize_NET返回:{nPacketSize:X8}", LogLevel.Error));
                }

                // ch:注册异常回调函数 | en:Register Exception Callback

                _nRet = _myCamera.RegisterExceptionCallBack(_ExceptionCallBack, IntPtr.Zero);
                if (CErrorDefine.MV_OK != _nRet)
                {
                    StatusMsg.OnNext(($"注册异常回调时发生错误，返回{_nRet:x8}!", LogLevel.Error));
                }
                StatusMsg.OnNext(("相机已连接", LogLevel.Info));
                ConnectStatus.OnNext(true);
                return Result.Ok();
            }
            ConnectStatus.OnNext(false);
            StatusMsg.OnNext(("相机连接失败", LogLevel.Error));
            return Result.Fail("相机连接失败");
        }
        ConnectStatus.OnNext(false);
        return Result.Fail("相机连接失败");
    }

    private void OnException(uint nMsgType, IntPtr pUser)
    {
        ConnectStatus.OnNext(false);

        StatusMsg.OnNext(($"当前设备异常！！！将自动释放设备,代码为{nMsgType}", LogLevel.Error));
        StopGrab();
        CloseDevices();
    }

    private void OnGrab(IntPtr pData, ref MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
    {
        try
        {
            var t1 = Cv2.GetCpuTickCount();
            if (IsMonoPixelFormat(pFrameInfo.enPixelType))
            {
                _grabMatTemp = new Mat(pFrameInfo.nHeight, pFrameInfo.nWidth, MatType.CV_8UC1, pData);
            }
            else if (pFrameInfo.enPixelType == MvGvspPixelType.PixelType_Gvsp_BayerGB8)
            {
                _grabMatTemp = new Mat(pFrameInfo.nHeight, pFrameInfo.nWidth, MatType.CV_8UC1, pData);
                _grabMatTemp = _grabMatTemp.CvtColor(ColorConversionCodes.BayerGB2BGR_VNG);
            }
            else if (pFrameInfo.enPixelType == MvGvspPixelType.PixelType_Gvsp_BGR8_Packed)
            {
                _grabMatTemp = new Mat(pFrameInfo.nHeight, pFrameInfo.nWidth, MatType.CV_8UC3, pData);
            }
            var t2 = Cv2.GetCpuTickCount();
            Debug.WriteLine($"grab image{pFrameInfo.nFrameNum},time:{(t2 - t1) / Cv2.GetTickFrequency() * 1000}ms");
            GrabMatSubject.OnNext(_grabMatTemp.Clone());
#if DEBUG
            //Cv2.ImShow("freegrab", _grabMatTemp);
            //Cv2.WaitKey(10);
#endif

            _grabMatTemp.Dispose();
        }
        catch (Exception ex)
        {
            StatusMsg.OnNext((ex.Message, LogLevel.Error));
            //throw ex;
        }
    }

    private static bool IsMonoPixelFormat(MvGvspPixelType enPixelType)
    {
        switch (enPixelType)
        {
            case MvGvspPixelType.PixelType_Gvsp_Mono1p:
            case MvGvspPixelType.PixelType_Gvsp_Mono2p:
            case MvGvspPixelType.PixelType_Gvsp_Mono4p:
            case MvGvspPixelType.PixelType_Gvsp_Mono8:
            case MvGvspPixelType.PixelType_Gvsp_Mono8_Signed:
            case MvGvspPixelType.PixelType_Gvsp_Mono10:
            case MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
            case MvGvspPixelType.PixelType_Gvsp_Mono12:
            case MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
            case MvGvspPixelType.PixelType_Gvsp_Mono14:
            case MvGvspPixelType.PixelType_Gvsp_Mono16:
                return true;

            default:
                return false;
        }
    }

    public Result SearchDevices()
    {
        _deviceListStr.Clear();
        int nRet = CSystem.EnumDevices(CSystem.MV_GIGE_DEVICE | CSystem.MV_USB_DEVICE, ref m_ltDeviceList);
        if (nRet == 0 && m_ltDeviceList.Count > 0)
        {
            for (int i = 0; i < m_ltDeviceList.Count; i++)
            {
                switch (m_ltDeviceList[i].nTLayerType)
                {
                    case CSystem.MV_GIGE_DEVICE:
                        var gigeInfo = (CGigECameraInfo)m_ltDeviceList[i];
                        if (gigeInfo.UserDefinedName != "")
                        {
                            _deviceListStr.Add($"GEV:{gigeInfo.UserDefinedName}({gigeInfo.chSerialNumber})");
                        }
                        else
                        {
                            _deviceListStr.Add($"GEV:{gigeInfo.chManufacturerName}{gigeInfo.chModelName}({gigeInfo.chSerialNumber})");
                        }
                        break;

                    case CSystem.MV_USB_DEVICE:
                        CUSBCameraInfo usbInfo = (CUSBCameraInfo)m_ltDeviceList[i];
                        if (usbInfo.UserDefinedName != "")
                        {
                            _deviceListStr.Add($"U3V:{usbInfo.UserDefinedName}({usbInfo.chSerialNumber})");
                        }
                        else
                        {
                            _deviceListStr.Add($"U#V:{usbInfo.chManufacturerName}{usbInfo.chModelName}({usbInfo.chSerialNumber})");
                        }
                        break;

                    default:
                        break;
                }
            }
            return Result.Ok();
        }
        else
        {
            ConnectStatus.OnNext(false);
            StatusMsg.OnNext(("未发现相机", LogLevel.Warn));
            return Result.Fail("未发现相机");
        }
    }

    public Result StartGrab()
    {
        _nRet = _myCamera.RegisterImageCallBackEx(_imageCallBack, IntPtr.Zero);

        if (_nRet != CErrorDefine.MV_OK)
        {
            StatusMsg.OnNext(($"注册回调函数失败，返回{_nRet:X8}", LogLevel.Error));
            return Result.Fail($"注册回调函数失败，返回{_nRet:X8}");
        }
        _nRet = _myCamera.StartGrabbing();
        if (_nRet != CErrorDefine.MV_OK)
        {
            StatusMsg.OnNext(($"开启抓图失败，返回{_nRet:X8}", LogLevel.Error));
            return Result.Fail($"开启抓图失败，返回{_nRet:X8}");
        }
        IsGrabing.OnNext(true);
        return Result.Ok();
    }

    public Result StopGrab()
    {
        _nRet = _myCamera.StopGrabbing();
        if (_nRet != CErrorDefine.MV_OK)
        {
            StatusMsg.OnNext(($"停止抓图失败，返回{_nRet:X8}", LogLevel.Error));
            return Result.Fail($"停止抓图失败，返回{_nRet:X8}");
        }
        IsGrabing.OnNext(false);
        return Result.Ok();
    }

    public Result SetupDevices()
    {
        // ch:设置采集连续模式 | en:Set Continues Aquisition Mode
        _myCamera.SetEnumValue("AcquisitionMode", (uint)MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
        _nRet = _myCamera.SetEnumValue("TriggerMode", (uint)MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
        if (_nRet != CErrorDefine.MV_OK)
        {
            StatusMsg.OnNext(($"设置触发模式失败,返回{_nRet:X8}", LogLevel.Error));
            return Result.Fail($"设置触发模式失败,返回{_nRet:X8}");
        }
        return Result.Ok();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _grabMatTemp?.Dispose();
            }
            try
            {
                Task.Run(() =>
                {
                    _nRet = _myCamera.DestroyHandle();
                    if (_nRet != CErrorDefine.MV_OK)
                    {
                        _nRet = _myCamera.DestroyHandle();
                    }
                    // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                    // TODO: 将大型字段设置为 null
                    _disposedValue = true;
                }).Wait(1000);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async Task<Result> FreeGrab(int milsec)
    {
        Func<Task> func = async () =>
        {
            _myCamera.SetEnumValue("TriggerMode", (uint)MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);

            StartGrab();
            await Task.Delay(milsec);

            StopGrab();
            _myCamera.SetEnumValue("TriggerMode", (uint)MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);
        };
        return await Result.Try(func);
    }
}