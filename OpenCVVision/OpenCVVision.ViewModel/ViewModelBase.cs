namespace OpenCVVision.ViewModel;

/// <summary>
/// ViewModel基类，IActivatableViewModel接口实现视图激活、抑制的订阅；IEnableLogger启用日志组件
/// </summary>
public class ViewModelBase : ReactiveObject, IActivatableViewModel, IEnableLogger
{
    public ViewModelActivator Activator { get; }

    /// <summary>
    /// viewmodel践行响应式的方式，大多数响应式的操作在SetupSubscriptions中配置，配置 流变化->观察->订阅(响应)通道，设计整个程序的逻辑
    /// </summary>
    public ViewModelBase()
    {
        Activator = new();
        //当视图激活时进行的操作
        this.WhenActivated(d =>
        {
            //当视图抑制时进行的操作
            Disposable.Create(() => SetupDeactivate())
                .DisposeWith(d);

            SetupCommands();
            SetupSubscriptions(d);
            SetupStart();
        });
    }

    /// <summary>
    /// 设置命令
    /// </summary>
    protected virtual void SetupCommands()
    {
    }

    /// <summary>
    /// 设置启动时加载
    /// </summary>
    protected virtual void SetupStart()
    {
    }

    /// <summary>
    /// 设置流订阅
    /// </summary>
    protected virtual void SetupSubscriptions(CompositeDisposable d)
    {
    }

    /// <summary>
    /// 设置视图切换离开时需要清理的内容
    /// </summary>
    protected virtual void SetupDeactivate()
    {
    }
}