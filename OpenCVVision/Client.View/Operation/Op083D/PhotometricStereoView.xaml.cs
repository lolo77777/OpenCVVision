using Client.ViewModel.Operation;

using ReactiveUI;

using System.Reactive.Disposables;

namespace Client.View.Operation
{
    /// <summary>
    /// PhotometricStereo1View.xaml 的交互逻辑
    /// </summary>
    public partial class PhotometricStereoView : ReactiveUserControl<PhotometricStereoViewModel>
    {
        public PhotometricStereoView()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.BindCommand(ViewModel, vm => vm.ViewPhotometricCommand, v => v.btnGotoSample).DisposeWith(d);
            });
        }
    }
}