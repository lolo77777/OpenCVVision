namespace OpenCVVision.View.Operation;

/// <summary>
/// GrayCodeView.xaml 的交互逻辑
/// </summary>
public partial class GrayCodeView
{
    public GrayCodeView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.BindCommand(ViewModel, vm => vm.View3dCommand, v => v.btnView3d).DisposeWith(d);
        });
    }
}