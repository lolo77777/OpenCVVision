using OpenCVVision.Contracts;
using OpenCVVision.Model;
using OpenCVVision.Services.Camera;
using OpenCVVision.Services.ImageProcess;

using SprinklerDetect.Services.HardWare.Camera;

namespace OpenCVVision.Services;

public class Register : RegisterBase
{
    public override void ConfigService()
    {
        _mutable.RegisterLazySingleton<IImageDataManager>(() => new ImageDataMemery());
        _mutable.RegisterLazySingleton(() => new LightPlaneCalibrate());
        _mutable.RegisterLazySingleton(() => new LightPlaneCal());

        _mutable.RegisterLazySingleton<IDisplayLog>(() => new DisplayLogService());
        _mutable.RegisterConstant<ICamera>(new HIKCamera(), "海康");
        //如果需要使用大恒相机，请自行安装大恒软件，并引用.net库。然后取消下面注释
        //_mutable.RegisterConstant<ICamera>(new DaHengCamera(), "大恒");
    }
}