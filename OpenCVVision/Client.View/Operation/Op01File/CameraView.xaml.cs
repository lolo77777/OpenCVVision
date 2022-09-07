namespace Client.View.Operation;

/// <summary>
/// CameraView.xaml 的交互逻辑
/// </summary>
public partial class CameraView : ReactiveUserControl<CameraViewModel>
{
    public CameraView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.BindCommand(ViewModel, vm => vm.SearchCommand, v => v.btnSearchCamera).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.CameraFactorys, v => v.cboxDeviceFactory.ItemsSource).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.CameraInfoList, v => v.cboxDeviceInfos.ItemsSource).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.ConnectCommand, v => v.btnConnect).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.StartGrabCommand, v => v.btnStart).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.CloseCommand, v => v.btnClose).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.PauseGrabCommand, v => v.btnPause).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.CanCamOpera, v => v.panelOpera.IsEnabled).DisposeWith(d);
            this.WhenAnyValue(x => x.cboxDeviceFactory.SelectedValue)
                .WhereNotNull()
                .Select(obj => obj.ToString())
                .BindTo(ViewModel, x => x.CameraFactory)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.cboxDeviceFactory.Items)
                .WhereNotNull()
                .Where(its => its.Count > 0)
                .Subscribe(_ => cboxDeviceFactory.SelectedIndex = 0)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.cboxDeviceInfos.Items)
                .WhereNotNull()
                .Where(its => its.Count > 0)
                .Subscribe(_ => cboxDeviceInfos.SelectedIndex = 0)
                .DisposeWith(d);
        });
    }
}