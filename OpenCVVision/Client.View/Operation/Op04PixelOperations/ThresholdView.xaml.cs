namespace Client.View.Operation
{
    /// <summary>
    /// ThresholdView.xaml 的交互逻辑
    /// </summary>
    public partial class ThresholdView : ReactiveUserControl<ThreshouldViewModel>
    {
        public ThresholdView()
        {
            InitializeComponent();
            SetupBinding();
        }

        private void SetupBinding()
        {
            this.WhenActivated(d =>
            {
                barChart.XAxes.ElementAt(0).MinStep = 2;
                barChart.XAxes.ElementAt(0).MaxLimit = 256;
                sliderMaxval.Value = 255;
                barChart.YAxes.ElementAt(0).MinStep = 2;
                this.OneWayBind(ViewModel, vm => vm.Series, v => v.barChart.Series).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ThreshouldModes, v => v.cbxThreshouldType.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Channels, v => v.cbxChannels.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.IsEnableEqualizeHist, v => v.cbxEqualizeHist.IsChecked).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ChanelSelectIndex, v => v.cbxChannels.SelectedIndex).DisposeWith(d);
                this.WhenAnyValue(x => x.cbxThreshouldType.SelectedValue)
                    .WhereNotNull()
                    .BindTo(ViewModel, x => x.ThresholdSelectValue)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderThresh.Value)
                    .WhereNotNull()
                    .BindTo(ViewModel, x => x.Thresh)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderMaxval.Value)
                    .WhereNotNull()
                    .BindTo(ViewModel, x => x.Maxval)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderThresh1.Value)
                    .WhereNotNull()
                    .BindTo(ViewModel, x => x.Thresh1)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderThresh2.Value)
                    .WhereNotNull()
                    .BindTo(ViewModel, x => x.Thresh2)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderThresh1.Value, x => x.sliderThresh2.Value)
                    .Throttle(TimeSpan.FromMilliseconds(200))
                    .Where(vt => vt.Item2 < vt.Item1)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(vt => sliderThresh2.Value = sliderThresh1.Value + 1)
                    .DisposeWith(d);
            });
        }
    }
}