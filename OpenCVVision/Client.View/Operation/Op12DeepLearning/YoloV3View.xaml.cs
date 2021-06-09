using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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
        private OpenFileDialog openFileDialog = new();

        public YoloV3View()
        {
            InitializeComponent();
            openFileDialog.Filter = "Weight files (*.weight)|*.weights;";
            SetupBinding();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "https://pjreddie.com/media/files/yolov3.weights");
        }

        private void SetupBinding()
        {
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.TxtImageFilePath, v => v.FilePathTextBox.Text).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.LoadImageCommand, v => v.btnLoadImage).DisposeWith(d);
                this.BindInteraction(ViewModel, vm => vm.LoadFileConfirm,
                   context => Observable.Return(openFileDialog.ShowDialog())
                   .Do(result =>
                   {
                       if (result.Equals(DialogResult.OK))
                       {
                           context.SetOutput(openFileDialog.FileName);
                       }
                       else
                       {
                           context.SetOutput(string.Empty);
                       }
                   }

               )).DisposeWith(d);
            });
        }
    }
}