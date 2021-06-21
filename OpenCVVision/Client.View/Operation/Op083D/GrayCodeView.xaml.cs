using Client.ViewModel.Operation;

using ReactiveUI;

using Splat;

using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Client.View.Operation
{
    /// <summary>
    /// GrayCodeView.xaml 的交互逻辑
    /// </summary>
    public partial class GrayCodeView : ReactiveUserControl<GrayCodeViewModel>
    {
        public GrayCodeView()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                btnView3d.Click += (o, e) =>
                {
                    View3DView view3DView = Locator.Current.GetService<View3DView>();
                    view3DView.ShowDialog();
                };
            });
        }
    }
}