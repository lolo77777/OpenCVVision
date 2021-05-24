using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Client.ViewModel.Operation.Op02ColorSpace;

using ReactiveUI;

namespace Client.View.Operation.Op02ColorSpace
{
    /// <summary>
    /// ColorSpaceView.xaml 的交互逻辑
    /// </summary>
    public partial class ColorSpaceView : ReactiveUserControl<ColorSpaceViewModel>
    {
        public ColorSpaceView()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.ColorModes, v => v.ColorModesCbx.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Channels, v => v.ChanelCbx.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ColorModeSelectInd, v => v.ColorModesCbx.SelectedIndex).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.ChannelSelectInd, v => v.ChanelCbx.SelectedIndex).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.CanOperat, v => v.cardMain.IsEnabled).DisposeWith(d);
            });
        }
    }
}