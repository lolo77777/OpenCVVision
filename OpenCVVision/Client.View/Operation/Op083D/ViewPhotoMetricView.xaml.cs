using Client.ViewModel.Operation;

using ReactiveUI;

using System.Diagnostics;
using System.Reactive.Disposables;
using System.Windows.Documents;

namespace Client.View.Operation
{
    /// <summary>
    /// ViewPhoteMetricView.xaml 的交互逻辑
    /// </summary>
    public partial class ViewPhotometricView : ReactiveUserControl<ViewPhotometricViewModel>
    {
        public ViewPhotometricView()
        {
            InitializeComponent();
            SetupBinding();
        }

        private void SetupBinding()
        {
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.NaviBackCommand, v => v.btnNaviBack).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.DisplayCommand, v => v.btnDisplay).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.SampleItems, v => v.cbxTestSample.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NormalmapWb, v => v.NormalmapImg.Source).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ZWb, v => v.ZImg.Source).DisposeWith(d);
                this.WhenAnyValue(x => x.cbxTestSample.SelectedIndex)
                    .BindTo(ViewModel, vm => vm.SampleSelectIndex)
                    .DisposeWith(d);
            });
        }

        private void Hyperlink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start("explorer.exe", Data.FilePath.Folder.PhotometricStereoFolder);
        }
    }
}