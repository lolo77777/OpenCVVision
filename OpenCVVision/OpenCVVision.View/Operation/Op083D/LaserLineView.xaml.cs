namespace OpenCVVision.View.Operation;

/// <summary>
/// LaserLineView.xaml 的交互逻辑
/// </summary>
public partial class LaserLineView
{
    public LaserLineView()
    {
        InitializeComponent();
        SetupBinding();
    }

    private void SetupBinding()
    {
        this.WhenActivated(d =>
        {
            this.BindCommand(ViewModel, vm => vm.CalibrateTestCommand, v => v.btnCalibrateTest).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.LaserLigthCalCommand, v => v.btnLaserLightCal).DisposeWith(d);
        });
    }
}