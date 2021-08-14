using Client.ViewModel;

using ReactiveUI;

using System.Diagnostics;
using System.Reactive.Disposables;

namespace Client.View
{
    [SingleInstanceView]
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : ReactiveUserControl<ShellViewModel>
    {
        public ShellView()
        {
            InitializeComponent();
            SetupBinding();
        }
        private void SetupBinding()
        {
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.NavigationViewModelSam, v => v.Nagivate.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ImageVMSam, v => v.ImgViewer.ViewModel).DisposeWith(d);
                //this.OneWayBind(ViewModel, vm => vm.Router, v => v.OperaPanel.Router).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.OperaVM, v => v.OperaPanel.ViewModel).DisposeWith(d);
#if DEBUG
                Debug.WriteLine($"ShellView的HashCode:{this.GetHashCode()}") ;
#endif

            });
        }
    }
}