namespace Client.View.Operation;

/// <summary>
/// MorphologyView.xaml 的交互逻辑
/// </summary>
public partial class MorphologyView
{
    public MorphologyView()
    {
        InitializeComponent();
        SetupBinding();
    }

    private void SetupBinding()
    {
        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel, vm => vm.MorphShapesItems, v => v.cbxMorphShapes.ItemsSource).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.MorphTypesItems, v => v.cbxMorphologyModes.ItemsSource).DisposeWith(d);
            this.WhenAnyValue(x => x.cbxMorphologyModes.SelectedValue)
                .WhereNotNull()
                .BindTo(ViewModel, x => x.MorphTypeSelectValue)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.cbxMorphShapes.SelectedValue)
                .WhereNotNull()
                .BindTo(ViewModel, x => x.MorphShapeSelectValue)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderKernelSizeX.Value)
                .BindTo(ViewModel, x => x.SizeX)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderKernelSizeY.Value)
                .BindTo(ViewModel, x => x.SizeY)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderKernelSizeX.Value)
                .BindTo(this, x => x.sliderKernelSizeY.Value)
                .DisposeWith(d);
        });
    }
}