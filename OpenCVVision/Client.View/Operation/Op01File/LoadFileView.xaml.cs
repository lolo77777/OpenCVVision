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

using Client.ViewModel.Operation.Op01File;

using ReactiveUI;

namespace Client.View.Operation._01File
{
    /// <summary>
    /// LoadFileView.xaml 的交互逻辑
    /// </summary>
    public partial class LoadFileView : ReactiveUserControl<LoadFileViewModel>
    {
        public LoadFileView()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.LoadImageCommand, v => v.btnLoadImage).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.TxtImageFilePath, v => v.FilePathTextBox.Text).DisposeWith(d);
            });
        }
    }
}