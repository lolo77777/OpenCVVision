namespace OpenCVVision.View.Operation;

/// <summary>
/// FeatureDetectionView.xaml 的交互逻辑
/// </summary>
public partial class FeatureDetectionView
{
    public FeatureDetectionView()
    {
        InitializeComponent();
        SetupBinding();
    }

    private void SetupBinding()
    {
        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel, vm => vm.FeatureDetectMethodItems, v => v.cbxFeatureDetectMethod.ItemsSource).DisposeWith(d);
            this.WhenAnyValue(x => x.cbxFeatureDetectMethod.SelectedValue)
                .WhereNotNull()
                .BindTo(ViewModel, x => x.FeatureDetectMethodSelectValue)
                .DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.ImageItems, v => v.cbxImageItems1.ItemsSource).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.ImageItems, v => v.cbxImageItems2.ItemsSource).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.MatchMethodItems, v => v.cbxMatchMethod.ItemsSource).DisposeWith(d);
            this.BindCommand(ViewModel, vm => vm.MatchCommand, v => v.btnImage).DisposeWith(d);
            this.WhenAnyValue(x => x.cbxImageItems1.SelectedValue)
                .WhereNotNull()
                .BindTo(ViewModel, x => x.FirstImageSelectValue)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.cbxImageItems2.SelectedValue)
                .WhereNotNull()
                .BindTo(ViewModel, vm => vm.SecondImageSelectValue)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.cbxMatchMethod.SelectedValue)
                .WhereNotNull()
                .BindTo(ViewModel, vm => vm.MatchMethod)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.cbIsEnableMinDis.IsChecked)
                .BindTo(ViewModel, vm => vm.IsEnableMinDis)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.cbIsEnableRANSAC.IsChecked)
                .BindTo(ViewModel, vm => vm.IsEnableRANSAC)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.cbIsEnableKnnMatch.IsChecked)
                .BindTo(ViewModel, vm => vm.IsEnableKnnMatch)
                .DisposeWith(d);
        });
    }
}