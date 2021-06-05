using System;
using System.Collections.Generic;
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

using Client.ViewModel.Operation.Op01File;

using ReactiveUI;

namespace Client.View.Operation._01File
{
    /// <summary>
    /// LoadFileView.xaml 的交互逻辑
    /// </summary>
    public partial class LoadFileView : ReactiveUserControl<LoadFileViewModel>
    {
        private OpenFileDialog openFileDialog = new();

        public LoadFileView()
        {
            InitializeComponent();
            openFileDialog.Filter = "Image files (*.jpg;*.bmp;*.png)|*.jpg;*.bmp;*.png";
            SetupBinding();
        }

        private void SetupBinding()
        {
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.LoadImageCommand, v => v.btnLoadImage).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.TxtImageFilePath, v => v.FilePathTextBox.Text).DisposeWith(d);
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