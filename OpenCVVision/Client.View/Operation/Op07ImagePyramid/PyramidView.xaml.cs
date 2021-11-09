namespace Client.View.Operation
{
    /// <summary>
    /// PyramidView.xaml 的交互逻辑
    /// </summary>
    public partial class PyramidView : ReactiveUserControl<PyramidViewModel>
    {
        public PyramidView()
        {
            InitializeComponent();
            SetupBinding();
        }

        private void SetupBinding()
        {
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.UpNumMax, v => v.sliderPyrup.Maximum).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.LaplaceCommand, v => v.btnLaplace).DisposeWith(d);
                this.WhenAnyValue(x => x.sliderLaplace.Value)
                    .BindTo(ViewModel, x => x.LaplaceNum)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderPyrDown.Value)
                    .BindTo(ViewModel, x => x.DownNum)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderPyrup.Value)
                    .BindTo(ViewModel, x => x.UpNum)
                    .DisposeWith(d);
            });
        }
    }
}