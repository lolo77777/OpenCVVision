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
    /// EdgeDetectView.xaml 的交互逻辑
    /// </summary>
    public partial class EdgeDetectView : ReactiveUserControl<EdgeDetectViewModel>
    {
        public EdgeDetectView()
        {
            InitializeComponent();
            SetupBinding();
        }

        private void SetupBinding()
        {
            this.WhenActivated(d =>
            {
                this.WhenAnyValue(x => x.sliderCannyDiam.Value)
                    .BindTo(ViewModel, x => x.KernelDiam)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderCannyThre1.Value)
                    .BindTo(ViewModel, x => x.Threshould1)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderCannyThre2.Value)
                    .BindTo(ViewModel, x => x.Threshould2)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.cbxIsL2gradient.IsChecked)
                    .BindTo(ViewModel, x => x.IsL2gradient)
                    .DisposeWith(d);
            });
        }
    }
}