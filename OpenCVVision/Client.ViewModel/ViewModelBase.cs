namespace Client.ViewModel;

public class ViewModelBase : ReactiveObject, IActivatableViewModel, IEnableLogger
{
    public ViewModelActivator Activator { get; }

    public ViewModelBase()
    {
        Activator = new();
        this.WhenActivated(d =>
        {
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