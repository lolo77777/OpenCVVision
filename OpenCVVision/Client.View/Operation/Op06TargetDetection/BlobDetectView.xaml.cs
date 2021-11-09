namespace Client.View.Operation
{
    /// <summary>
    /// BlobDetectView.xaml 的交互逻辑
    /// </summary>
    public partial class BlobDetectView : ReactiveUserControl<BlobDetectViewModel>
    {
        public BlobDetectView()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.MinDistBetweenBlobsSam, v => v.txtMinDistBetweenBlobs.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ThresholdStepSam, v => v.txtThresholdStep.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.MinThresholdSam, v => v.txtMinThreshold.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.MaxThresholdSam, v => v.txtMaxThreshold.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.IsEnableArea, v => v.cbxArea.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.MinAreaSam, v => v.txtMinArea.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.MaxAreaSam, v => v.txtMaxArea.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.IsEnableCircularity, v => v.cbxCircularity.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.MinCircularitySam, v => v.txtMinCircularity.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.MaxCircularitySam, v => v.txtMaxCircularity.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.IsEnableColor, v => v.cbxColor.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.BlobColorSam, v => v.txtBlobColor.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.IsEnableConvexity, v => v.cbxConvexity.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.MinConvexitySam, v => v.txtMinConvexity.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.MaxConvexitySam, v => v.txtMaxConvexity.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.IsEnableInertia, v => v.cbxInertiaRatio.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.MinInertiaRatioSam, v => v.txtMinInertiaRatio.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.MaxInertiaRatioSam, v => v.txtMaxInertiaRatio.Text).DisposeWith(d);
            });
        }
    }
}