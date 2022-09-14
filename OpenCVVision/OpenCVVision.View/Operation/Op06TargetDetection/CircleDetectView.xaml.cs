namespace OpenCVVision.View.Operation;

/// <summary>
/// CircleDetectView.xaml 的交互逻辑
/// </summary>
public partial class CircleDetectView
{
    public CircleDetectView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.Bind(ViewModel, vm => vm.Dp, v => v.txtDp.Text).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.MinDist, v => v.txtMinDist.Text).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.CannyParam, v => v.txtCannyParam.Text).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.Param2, v => v.txtParam2.Text).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.MinRadius, v => v.txtMinRadius.Text).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.MaxRadius, v => v.txtMaxRadius.Text).DisposeWith(d);

            this.OneWayBind(ViewModel, vm => vm.CircleItems, v => v.cbxCircleItems.ItemsSource).DisposeWith(d);
            this.WhenAnyValue(x => x.cbxCircleItems.SelectedValue)
                .WhereNotNull()
                .BindTo(ViewModel, x => x.SelectCircleValue);
        });
    }
}