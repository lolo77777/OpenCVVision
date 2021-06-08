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