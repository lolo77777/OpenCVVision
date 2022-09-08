namespace Client.View;

/// <summary>
/// LightAndDarkFilterView.xaml 的交互逻辑
/// </summary>
public partial class LightAndDarkFilterView
{
    public LightAndDarkFilterView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel, vm => vm.LightAndDarkType, v => v.cbxFilterType.ItemsSource).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.TypeSelectIndexed, v => v.cbxFilterType.SelectedIndex).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.KernelSize, v => v.txtBoxWindowSize.Text).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.SizeWidth, v => v.txtBoxWidth.Text).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.SizeHeight, v => v.txtBoxHeight.Text).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.LightScale, v => v.sliderLightValue.Value).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.DarkScale, v => v.sliderDarkValue.Value).DisposeWith(d);
            this.WhenAnyValue(x => x.cbxFilterType.SelectedIndex)
                .Subscribe(i =>
                {
                    switch (i)
                    {
                        case 0:
                            darkGrid.IsEnabled = true;
                            lightGrid.IsEnabled = false;
                            break;

                        case 1:
                            darkGrid.IsEnabled = false;
                            lightGrid.IsEnabled = true;
                            break;

                        case 2:
                            darkGrid.IsEnabled = true;
                            lightGrid.IsEnabled = true;
                            break;

                        default:
                            break;
                    }
                }
                );
        });
    }
}