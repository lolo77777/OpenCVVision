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
        _mutable.RegisterConstant<ICamera>(new DaHengCamera(), "大恒");
    }
}