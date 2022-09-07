using Client.Common;
using Client.Services.Camera;

using SprinklerDetect.Services.HardWare.Camera;

namespace Client.Services
{
    public class Register : RegisterBase
    {
        public override void ConfigService()
        {
            _mutable.RegisterConstant<ICamera>(new HIKCamera(), "海康");
            _mutable.RegisterConstant<ICamera>(new DaHengCamera(), "大恒");
        }
    }
}