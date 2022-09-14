namespace OpenCVVision.View.Operation;

/// <summary>
/// PhotometricStereo1View.xaml 的交互逻辑
/// </summary>
public partial class PhotometricStereoView
{
    public PhotometricStereoView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.BindCommand(ViewModel, vm => vm.ViewPhotometricCommand, v => v.btnGotoSample).DisposeWith(d);
        });
    }
}