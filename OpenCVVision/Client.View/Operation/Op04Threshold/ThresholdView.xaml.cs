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
                this.OneWayBind(ViewModel, vm => vm.BarViewModelSam, v => v.BarVMViewHost.ViewModel).DisposeWith(d);
            });
        }
    }
}