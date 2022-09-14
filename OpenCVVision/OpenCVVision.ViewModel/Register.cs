using OpenCVVision.Model;

namespace OpenCVVision.ViewModel;

internal class Register : RegisterBase
{
    /// <summary>
    /// 通过ViewModel的特性信息，注册operationViewModel对应的不同的ViewModel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private void RegistOperationViewModel<T>() where T : IOperationViewModel, new()
    {
        _mutable.Register<IOperationViewModel>(() => new T(), OpStaticMethod.GetOpInfo<T>().info);
    }

    /// <summary>
    ///通过特性信息，提供相应的ViewModel服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T ResolveVM<T>() where T : IOperationViewModel
    {
        return (T)Locator.Current.GetService<IOperationViewModel>(OpStaticMethod.GetOpInfo<T>().info);
    }

    /// <summary>
    /// 注册所有viewmodel
    /// </summary>
    public override void ConfigService()
    {
        _mutable.RegisterLazySingleton(() => new NavigationViewModel());
        _mutable.RegisterLazySingleton(() => new ImageViewModel());
        _mutable.RegisterLazySingleton(() => new ShellViewModel());
        RegistOperationViewModel<LoadFileViewModel>();
        RegistOperationViewModel<CameraViewModel>();
        RegistOperationViewModel<ColorSpaceViewModel>();
        RegistOperationViewModel<FilterViewModel>();
        RegistOperationViewModel<LightAndDarkFilterViewModel>();
        RegistOperationViewModel<ThreshouldViewModel>();
        RegistOperationViewModel<MorphologyViewModel>();
        RegistOperationViewModel<ConnectedComponentsViewModel>();
        RegistOperationViewModel<ContoursViewModel>();
        RegistOperationViewModel<RoiViewModel>();
        RegistOperationViewModel<LaserLineViewModel>();
        RegistOperationViewModel<PyramidViewModel>();
        RegistOperationViewModel<EdgeDetectViewModel>();
        RegistOperationViewModel<YoloViewModel>();
        RegistOperationViewModel<PaddleXViewModel>();
        RegistOperationViewModel<WechatQRCodeViewModel>();
        RegistOperationViewModel<FeatureDetectionViewModel>();
        //RegistLazySingletonOpVM<MatchTemplateViewModel>();
        RegistOperationViewModel<GrayCodeViewModel>();
        _mutable.Register(() => new View3DViewModel());
        RegistOperationViewModel<PhotometricStereoViewModel>();
        _mutable.RegisterLazySingleton(() => new ViewPhotometricViewModel());
        _mutable.Register(() => new ImageToolViewModel());
        RegistOperationViewModel<TwoMatOperationViewModel>();
        RegistOperationViewModel<CircleDetectViewModel>();
        RegistOperationViewModel<BlobDetectViewModel>();
    }
}