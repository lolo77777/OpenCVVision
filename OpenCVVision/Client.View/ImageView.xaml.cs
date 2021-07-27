using Client.ViewModel;

using ReactiveUI;

using System.Reactive.Disposables;

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
            SetupBinding();
        }

        private void SetupBinding()
        {
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.HistoryItems, v => v.HistoryImg.ItemsSource).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.InputImageVM, v => v.InPutImgVM.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.OutputImageVM, v => v.OutPutImgVM.ViewModel).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.HistoryItemSelectInd, v => v.HistoryImg.SelectedIndex).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.AddOutputImgToImgManagerCommand, v => v.btnAddOutputImage).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.OutputImageMarkTxt, v => v.OutputMarkTxtBox.Text).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RemoveImgFromImgManagerCommand, v => v.btnRemoveImage).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.Time, v => v.lblTime.Text).DisposeWith(d);
            });
        }
    }
}