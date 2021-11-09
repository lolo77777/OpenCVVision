namespace Client.Model
{
    internal class Register : RegisterBase
    {
        public override void ConfigService()
        {
            _mutable.RegisterLazySingleton<IImageDataManager>(() => new ImageDataMemery());
            _mutable.RegisterLazySingleton(() => new LightPlaneCalibrate());
            _mutable.RegisterLazySingleton(() => new LightPlaneCal());
        }
    }
}