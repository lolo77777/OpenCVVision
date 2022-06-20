namespace Client.View.Operation;

/// <summary>
/// RoiView.xaml 的交互逻辑
/// </summary>
public partial class RoiView
{
    public RoiView()
    {
        InitializeComponent();
        SetupBinding();
    }

    private void SetupBinding()
    {
        this.WhenActivated(d =>
        {
            this.OneWayBind(ViewModel, vm => vm.WidthLimit, v => v.sliderWidth.Maximum).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.WidthLimit, v => v.sliderLeft.Maximum).DisposeWith(d);
            this.OneWayBind(ViewModel, vm => vm.HeightLimit, v => v.sliderHeight.Maximum).DisposeWith(d);

            this.OneWayBind(ViewModel, vm => vm.HeightLimit, v => v.sliderTop.Maximum).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.Height, v => v.sliderHeight.Value).DisposeWith(d);
            this.Bind(ViewModel, vm => vm.Width, v => v.sliderWidth.Value).DisposeWith(d);

            this.WhenAnyValue(x => x.RoiList.SelectedValue)
                .WhereNotNull()
                .Select(t => ((ListBoxItem)t).Content.ToString())
                .BindTo(ViewModel, x => x.RoiModeSelectValue)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderTop.Value)
                .BindTo(ViewModel, x => x.Top)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderLeft.Value)
                .BindTo(ViewModel, x => x.Left)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderLeft.Value)
                .Where(i => i + sliderWidth.Value > sliderWidth.Maximum)
                .Subscribe(i => sliderWidth.Value = sliderWidth.Maximum - i)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderTop.Value)
                .Where(i => i + sliderHeight.Value > sliderHeight.Maximum)
                .Subscribe(i => sliderHeight.Value = sliderHeight.Maximum - i)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderHeight.Value)
                .Where(i => sliderTop.Value + i > sliderHeight.Maximum)
                .Subscribe(i => sliderTop.Value = sliderHeight.Maximum - i)
                .DisposeWith(d);
            this.WhenAnyValue(x => x.sliderWidth.Value)
                .Where(i => sliderLeft.Value + i > sliderWidth.Maximum)
                .Subscribe(i => sliderLeft.Value = sliderWidth.Maximum - i)
                .DisposeWith(d);
        });
    }
}