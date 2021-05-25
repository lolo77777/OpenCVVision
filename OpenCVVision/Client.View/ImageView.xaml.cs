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

using Client.ViewModel;

using ReactiveUI;

namespace Client.View
{
    /// <summary>
    /// ImageView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageView : ReactiveUserControl<ImageViewModel>
    {
        public ImageView()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.HistoryItems, v => v.HistoryImg.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.InputImg, v => v.InputImg.Source).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.OutputImg, v => v.OutputImg.Source).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.HistoryItemSelectInd, v => v.HistoryImg.SelectedIndex).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.AddOutputImgToImgManagerCommand, v => v.btnAddOutputImage).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.OutputImageMarkTxt, v => v.OutputMarkTxtBox.Text).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RemoveImgFromImgManagerCommand, v => v.btnRemoveImage).DisposeWith(d);
            });
        }
    }
}