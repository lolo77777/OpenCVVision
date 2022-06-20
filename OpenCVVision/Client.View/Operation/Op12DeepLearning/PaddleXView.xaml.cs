namespace Client.View.Operation.Op12DeepLearning;

/// <summary>
/// PaddleXView.xaml 的交互逻辑
/// </summary>
public partial class PaddleXView
{
    public PaddleXView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.BindCommand(ViewModel, vm => vm.LoadClsModelCommand, v => v.btnClsLoad).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.DestoryClsModelCommand, v => v.btnClsDestory).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.LoadDetModelCommand, v => v.btnDetLoad).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.DestoryDetModelCommand, v => v.btnDetDestory).DisposeWith(d);

            this.OneWayBind(ViewModel, vm => vm.ClsLoaded, v => v.btnClsLoad.IsEnabled, b => !b).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.ClsLoaded, v => v.btnClsDestory.IsEnabled).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.DetLoaded, v => v.btnDetLoad.IsEnabled, b => !b).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.DetLoaded, v => v.btnDetDestory.IsEnabled).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.ClsLoaded, v => v.panelDet.IsEnabled, b => !b).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.DetLoaded, v => v.panelCls.IsEnabled, b => !b).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.ClsResult, v => v.txtClsResult.Text).DisposeWith(d);
        });
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe", "https://gitee.com/paddlepaddle/PaddleX/tree/develop/deploy/cpp/docs/csharp_deploy");
    }
}