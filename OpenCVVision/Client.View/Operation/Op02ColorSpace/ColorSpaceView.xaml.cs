using Client.ViewModel.Operation;

using ReactiveUI;

using System.Reactive.Disposables;

namespace Client.View.Operation
{
    /// <summary>
    /// ColorSpaceView.xaml 的交互逻辑
    /// </summary>
    public partial class ColorSpaceView : ReactiveUserControl<ColorSpaceViewModel>
    {
        public ColorSpaceView()
        {
            InitializeComponent();
            SetupBinding();
        }

        private void SetupBinding()
        {
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.ColorModes, v => v.ColorModesCbx.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Channels, v => v.ChanelCbx.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ColorModeSelectInd, v => v.ColorModesCbx.SelectedIndex).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ChannelSelectInd, v => v.ChanelCbx.SelectedIndex).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.CanOperat, v => v.cardMain.IsEnabled).DisposeWith(d);
                this.WhenAnyValue(x => x.isEnableInverse.IsChecked)
                    .BindTo(ViewModel, x => x.IsEnableInverse)
                    .DisposeWith(d);
            });
        }
    }
}