namespace Client.View.Operation;

/// <summary>
/// EdgeDetectView.xaml 的交互逻辑
/// </summary>
public partial class EdgeDetectView
{
    public EdgeDetectView()
    {
        InitializeComponent();
        SetupBinding();
    }

    private void SetupBinding()
    {
        this.WhenActivated(d =>
        {
            this.WhenAnyValue(x => x.sliderCannyDiam.Value)
                .BindTo(ViewModel, x => x.KernelDiam)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderCannyThre1.Value)
                .BindTo(ViewModel, x => x.Threshould1)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderCannyThre2.Value)
                .BindTo(ViewModel, x => x.Threshould2)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.cbxIsL2gradient.IsChecked)
                .BindTo(ViewModel, x => x.IsL2gradient)
                .DisposeWith(d);
        });
    }
}