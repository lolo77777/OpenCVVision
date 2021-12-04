namespace Client.View;

[SingleInstanceView]
public partial class ShellView : ReactiveUserControl<ShellViewModel>
{
    public ShellView()
    {
        InitializeComponent();
        SetupBinding();
    }

    /// <summary>
    /// 通过rxui的方式设置绑定，调用whenactivate可以在视图激活显示时开启绑定，视图不激活的时候停止绑定
    /// </summary>
    private void SetupBinding()
    {
        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel, vm => vm.NavigationViewModelSam, v => v.Nagivate.ViewModel).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.ImageVMSam, v => v.ImgViewer.ViewModel).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.OperaVM, v => v.OperaPanel.ViewModel).DisposeWith(d);
        });
    }
}