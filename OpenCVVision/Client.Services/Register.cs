using Client.Common;
using Client.Contracts;
using Client.Services.Camera;
using Client.Services.ImageProcess;

using SprinklerDetect.Services.HardWare.Camera;

namespace Client.Services;

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