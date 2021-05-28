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

using Client.ViewModel.Operation;

using Microsoft.Windows.Themes;

using ReactiveUI;

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
            this.WhenActivated(d =>
            {
                barChart.XAxes.ElementAt(0).MinStep = 2;
                barChart.XAxes.ElementAt(0).MaxLimit = 256;
                sliderMaxval.Value = 255;
                barChart.YAxes.ElementAt(0).MinStep = 2;
                this.OneWayBind(ViewModel, vm => vm.Series, v => v.barChart.Series).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ThreshouldModes, v => v.cbxThreshouldType.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Channels, v => v.cbxChannels.ItemsSource).DisposeWith(d);
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
            });
        }
    }
}