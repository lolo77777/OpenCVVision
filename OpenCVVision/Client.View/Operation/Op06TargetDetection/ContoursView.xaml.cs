namespace Client.View.Operation;

/// <summary>
/// ContoursView.xaml 的交互逻辑
/// </summary>
public partial class ContoursView
{
    public ContoursView()
    {
        InitializeComponent();
        SetupBinding();
    }

    private void SetupBinding()
    {
        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel, vm => vm.RetrievalModesStr, v => v.cbxRetrievalModes.ItemsSource).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.ContourApproximationModesStr, v => v.cbxContourApproximationModes.ItemsSource).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.ContourIdItems, v => v.cbxContourIdItems.ItemsSource).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.BoundingShapesItems, v => v.cbxBoundingShapes.ItemsSource).DisposeWith(d);
            this.WhenAnyValue(x => x.cbxRetrievalModes.SelectedValue)
                .WhereNotNull()
                .BindTo(ViewModel, x => x.RetrievalSelectValue)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.cbxContourApproximationModes.SelectedValue)
                .WhereNotNull()
                .BindTo(ViewModel, x => x.ContourApproximationSelectValue)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.cbxContourIdItems.SelectedValue)
                .WhereNotNull()
                .BindTo(ViewModel, x => x.ContourIdItemSelectValue)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.cbxBoundingShapes.SelectedValue)
                .WhereNotNull()
                .BindTo(ViewModel, x => x.BoundingShapeSelectValue)
                .DisposeWith(d);
        });
    }
}