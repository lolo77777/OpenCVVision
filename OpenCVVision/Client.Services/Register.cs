using Client.Common;
using Client.Contracts;
using Client.Services.Camera;

using SprinklerDetect.Services.HardWare.Camera;

namespace Client.Services
{
    public class Register : RegisterBase
    {
        public override void ConfigService()
        {
            ////注入NLog
            //_mutable.RegisterConstant(LogManager.GetLogger("*"));
            ////使用自定义的log注入ILogger，在自定义的log中使用NLog写入文件、控制台输出；同时写入日志表格
            //_mutable.RegisterLazySingleton<Splat.ILogger>(() => new ObservableLogger());

            _mutable.RegisterLazySingleton<IDisplayLog>(() => new DisplayLogService());
            _mutable.RegisterConstant<ICamera>(new HIKCamera(), "海康");
            _mutable.RegisterConstant<ICamera>(new DaHengCamera(), "大恒");
        }
    }
}