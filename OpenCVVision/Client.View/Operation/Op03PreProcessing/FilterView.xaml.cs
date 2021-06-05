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

using Client.ViewModel.Operation.Op03PreProcessing;

using ReactiveUI;

namespace Client.View.Operation.Op03PreProcessing
{
    /// <summary>
    /// FilterView.xaml 的交互逻辑
    /// </summary>
    public partial class FilterView : ReactiveUserControl<FilterViewModel>
    {
        public FilterView()
        {
            InitializeComponent();
            SetupBinding();
        }

        private void SetupBinding()
        {
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.FilterModes, v => v.cbxFilterType.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.FilterModeSelectIndex, v => v.cbxFilterType.SelectedIndex).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.BolSigmaIsEnable, v => v.txtBoxSigmaX.IsEnabled).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.BolSigmaIsEnable, v => v.txtBoxSigmaY.IsEnabled).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.BolSizeYIsEnable, v => v.sliderKernelSizeY.IsEnabled).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CanOperat, v => v.cardMain.IsEnabled).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.BolSigmaColorAndSpace, v => v.BilateralFilterPanel.IsEnabled).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.BolSizeIsEnable, v => v.sliderKernelSizeX.IsEnabled).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.BolSizeIsEnable, v => v.sliderKernelSizeY.IsEnabled).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SigmaX, v => v.txtBoxSigmaX.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SigmaY, v => v.txtBoxSigmaY.Text).DisposeWith(d);

                this.WhenAnyValue(x => x.sliderKernelSizeX.Value)
                    .BindTo(ViewModel, x => x.SizeX)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderKernelSizeY.Value)
                    .BindTo(ViewModel, x => x.SizeY)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderKernelSizeX.Value)
                    .BindTo(this, x => x.sliderKernelSizeY.Value)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderKernelDiam.Value)
                    .BindTo(ViewModel, x => x.KernelDiam)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.slidersigmaColor.Value)
                    .BindTo(ViewModel, x => x.SigmaColor)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.slidersigmaSpace.Value)
                    .BindTo(ViewModel, x => x.SigmaSpace)
                    .DisposeWith(d);
            });
        }
    }
}