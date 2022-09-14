using System.Diagnostics;

namespace OpenCVVision.Services.Camera;

public class HIKCamera : ICamera
{
    private MyCamera.MV_CC_DEVICE_INFO_LIST _deviceList;
    private MyCamera _myCamera;
    private int _nRet;
    private bool _disposedValue;
    private readonly MyCamera.cbExceptiondelegate _lossConnectCallBack;
    private readonly MyCamera.cbOutputExdelegate _imageCallBack;
    private int _selectDevice;
    private Mat _grabMatTemp = new();
    private readonly IntPtr _pBufForConvert = IntPtr.Zero;
    private MyCamera.MV_PIXEL_CONVERT_PARAM _stConvertPixelParam = new();
    private readonly List<string> _deviceListStr = new();
    public ReadOnlyCollection<string> DeviceListStr { get; }
    public ReplaySubject<(string, LogLevel)> StatusMsg { get; } = new(100, TimeSpan.FromMilliseconds(60));
    public Subject<Mat> GrabMatSubject { get; } = new();
    public BehaviorSubject<bool> ConnectStatus { get; } = new(false);
    public BehaviorSubject<bool> IsGrabing { get; } = new(false);

    public HIKCamera()
    {
        _myCamera = new MyCamera();
        _imageCallBack = new MyCamera.cbOutputExdelegate(OnGrab);
        _lossConnectCallBack = new MyCamera.cbExceptiondelegate(OnLossConnect);
        DeviceListStr = new ReadOnlyCollection<string>(_deviceListStr);
    }

    public Result CloseDevices()
    {
        _nRet = _myCamera.MV_CC_CloseDevice_NET();
        if (_nRet != MyCamera.MV_OK)
        {
            StatusMsg.OnNext(($"关闭设备失败，返回{_nRet:X8}", LogLevel.Error));
            return Result.Fail($"关闭设备失败，返回{_nRet:X8}");
        }
        Marshal.FreeHGlobal(_pBufForConvert);
        ConnectStatus.OnNext(false);
        IsGrabing.OnNext(false);
        return Result.Ok();
    }

    public Result ConnectDevices(int selectIndex = 0)
    {
        if (_deviceList.nDeviceNum > 0 && DeviceListStr.Any() && selectIndex != -1)
        {
            _selectDevice = selectIndex;
            var deviceObj = Marshal.PtrToStructure(_deviceList.pDeviceInfo[selectIndex], typeof(MyCamera.MV_CC_DEVICE_INFO));
            if (deviceObj != null)
            {
                var device = (MyCamera.MV_CC_DEVICE_INFO)deviceObj;
                _myCamera = new MyCamera();
                _nRet = _myCamera.MV_CC_CreateDevice_NET(ref device);
                if (_nRet != MyCamera.MV_OK)
                {
                    ConnectStatus.OnNext(false);
                    return Result.Fail("创建相机设备类型失败！");
                }
                _nRet = _myCamera.MV_CC_OpenDevice_NET();
                if (_nRet != MyCamera.MV_OK)
                {
                    _myCamera.MV_CC_DestroyDevice_NET();
                    StatusMsg.OnNext(($"设备打开失败!MV_CC_OpenDevice_NET返回:{_nRet:X8}", LogLevel.Error));
                    ConnectStatus.OnNext(false);
                    return Result.Fail($"设备打开失败!MV_CC_OpenDevice_NET返回:{_nRet:X8}");
                }
                // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only
                // works for the GigE camera)
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    int nPacketSize = _myCamera.MV_CC_GetOptimalPacketSize_NET();
                    if (nPacketSize > 0)
                    {
                        _nRet = _myCamera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                        if (_nRet != MyCamera.MV_OK)
                        {
                            StatusMsg.OnNext(($"Set Packet Size failed! MV_CC_SetIntValue_NET返回:{_nRet:X8}", LogLevel.Error));
                        }
                    }
                    else
                    {
                        StatusMsg.OnNext(($"Get Packet Size failed! MV_CC_GetOptimalPacketSize_NET返回:{nPacketSize:X8}", LogLevel.Error));
                    }
                }

                // ch:注册异常回调函数 | en:Register Exception Callback

                _nRet = _myCamera.MV_CC_RegisterExceptionCallBack_NET(_lossConnectCallBack, IntPtr.Zero);
                if (MyCamera.MV_OK != _nRet)
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

    private void OnLossConnect(uint nMsgType, IntPtr pUser)
    {
        ConnectStatus.OnNext(false);
        StatusMsg.OnNext(("相机触发掉线异常！", LogLevel.Error));
        if (nMsgType == MyCamera.MV_EXCEPTION_DEV_DISCONNECT)
        {
            StatusMsg.OnNext(("当前设备已失去连接！！！将自动释放设备", LogLevel.Info));
            StopGrab();
            CloseDevices();
        }
    }

    private void OnGrab(IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
    {
        try
        {
            var t1 = Cv2.GetCpuTickCount();
            if (IsMonoPixelFormat(pFrameInfo.enPixelType))
            {
                _grabMatTemp = new Mat(pFrameInfo.nHeight, pFrameInfo.nWidth, MatType.CV_8UC1, pData);
            }
            else if (pFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB8)
            {
                _grabMatTemp = new Mat(pFrameInfo.nHeight, pFrameInfo.nWidth, MatType.CV_8UC1, pData);
                _grabMatTemp = _grabMatTemp.CvtColor(ColorConversionCodes.BayerGB2BGR_VNG);
            }
            else if (pFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed)
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

    private void ConvertColor(MyCamera myCamera, IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo, ref IntPtr ptrBufferConvert)
    {
        if (ptrBufferConvert == IntPtr.Zero)
        {
            ptrBufferConvert = Marshal.AllocHGlobal(pFrameInfo.nWidth * pFrameInfo.nHeight * 3);
        }

        _stConvertPixelParam.nWidth = pFrameInfo.nWidth;
        _stConvertPixelParam.nHeight = pFrameInfo.nHeight;
        _stConvertPixelParam.pSrcData = pData;
        _stConvertPixelParam.nSrcDataLen = pFrameInfo.nFrameLen;
        _stConvertPixelParam.enSrcPixelType = pFrameInfo.enPixelType;
        _stConvertPixelParam.enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
        _stConvertPixelParam.pDstBuffer = ptrBufferConvert;
        _stConvertPixelParam.nDstBufferSize = (uint)(pFrameInfo.nWidth * pFrameInfo.nHeight * 3);

        var nRet = myCamera.MV_CC_ConvertPixelType_NET(ref _stConvertPixelParam);
    }

    private static bool IsMonoPixelFormat(MyCamera.MvGvspPixelType enType)
    {
        return enType switch
        {
            MyCamera.MvGvspPixelType.PixelType_Gvsp_HB_Mono8 or MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8 or MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8_Signed or MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10 or MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed or MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12 or MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed => true,
            _ => false,
        };
    }

    public Result SearchDevices()
    {
        _deviceListStr.Clear();
        int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref _deviceList);
        if (nRet == MyCamera.MV_OK && _deviceList.nDeviceNum > 0)
        {
            for (int i = 0; i < _deviceList.nDeviceNum; i++)
            {
                var deviceObj = Marshal.PtrToStructure(_deviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));

                if (deviceObj != null)
                {
                    var device = (MyCamera.MV_CC_DEVICE_INFO)deviceObj;
                    switch (device.nTLayerType)
                    {
                        case MyCamera.MV_GIGE_DEVICE:
                            var gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                            if (gigeInfo.chUserDefinedName != "")
                            {
                                _deviceListStr.Add($"GEV:{gigeInfo.chUserDefinedName}({gigeInfo.chSerialNumber})");
                            }
                            else
                            {
                                _deviceListStr.Add($"GEV:{gigeInfo.chManufacturerName}{gigeInfo.chModelName}({gigeInfo.chSerialNumber})");
                            }
                            break;

                        case MyCamera.MV_USB_DEVICE:
                            var usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(device.SpecialInfo.stUsb3VInfo, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                            if (usbInfo.chUserDefinedName != null)
                            {
                                _deviceListStr.Add($"U3V:{usbInfo.chUserDefinedName}({usbInfo.chSerialNumber})");
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
        _nRet = _myCamera.MV_CC_RegisterImageCallBackEx_NET(_imageCallBack, IntPtr.Zero);

        if (_nRet != MyCamera.MV_OK)
        {
            StatusMsg.OnNext(($"注册回调函数失败，返回{_nRet:X8}", LogLevel.Error));
            return Result.Fail($"注册回调函数失败，返回{_nRet:X8}");
        }
        _nRet = _myCamera.MV_CC_StartGrabbing_NET();
        if (_nRet != MyCamera.MV_OK)
        {
            StatusMsg.OnNext(($"开启抓图失败，返回{_nRet:X8}", LogLevel.Error));
            return Result.Fail($"开启抓图失败，返回{_nRet:X8}");
        }
        IsGrabing.OnNext(true);
        return Result.Ok();
    }

    public Result StopGrab()
    {
        _nRet = _myCamera.MV_CC_StopGrabbing_NET();
        if (_nRet != MyCamera.MV_OK)
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
        _myCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", (uint)MyCamera.MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
        _nRet = _myCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
        if (_nRet != MyCamera.MV_OK)
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
                    _nRet = _myCamera.MV_CC_DestroyDevice_NET();
                    if (_nRet != MyCamera.MV_OK)
                    {
                        _nRet = _myCamera.MV_CC_DestroyDevice_NET();
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
        return await Result.Try(async () =>
        {
            _myCamera.MV_CC_SetEnumValueByString_NET("TriggerMode", "Off");

            StartGrab();
            await Task.Delay(milsec);

            StopGrab();
            _myCamera.MV_CC_SetEnumValueByString_NET("TriggerMode", "On");
        });
    }
}