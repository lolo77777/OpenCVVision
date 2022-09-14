namespace OpenCVVision.View.Operation;

/// <summary>
/// WechatQRCode.xaml 的交互逻辑
/// </summary>
public partial class WechatQRCodeView
{
    public WechatQRCodeView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.BindCommand(ViewModel, vm => vm.LoadModelCommand, v => v.btnClsLoad).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.DestroyModelCommand, v => v.btnClsDestory).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.IsLoaded, v => v.btnClsLoad.IsEnabled, b => !b).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.IsLoaded, v => v.btnClsDestory.IsEnabled).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.DetectResult, v => v.txtClsResult.Text).DisposeWith(d);
        });
    }
}