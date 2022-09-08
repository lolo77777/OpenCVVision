using System.Windows.Media;

namespace Client.View;

[SingleInstanceView]
public partial class ShellView
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
            this.OneWayBind(ViewModel, vm => vm.MsgLogLevel, v => v.txtBorder.Background, vmToViewConverter).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.LogMsg, v => v.txtInfo.Text).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.LogDataInfo, v => v.dataGridLogInfo.ItemsSource).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.LevelSelectIndex, v => v.listboxLevel.SelectedIndex).DisposeWith(d);
        });
    }

    private Brush vmToViewConverter(Splat.LogLevel level)
    {
        return (int)level switch
        {
            < 3 => Brushes.LightGreen,
            3 => Brushes.Orange,
            > 3 => Brushes.Red
        };
    }
}