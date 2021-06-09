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
    /// YoloV3View.xaml 的交互逻辑
    /// </summary>
    public partial class YoloV3View : ReactiveUserControl<YoloV3ViewModel>
    {
        public YoloV3View()
        {
            InitializeComponent();
            SetupBinding();
        }

        private void SetupBinding()
        {
            this.WhenActivated(d =>
            {
                //this.BindCommand(ViewModel, vm => vm.DetectCommand, v => v.btnDetectTest).DisposeWith(d);
            });
        }
    }
}