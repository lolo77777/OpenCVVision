using Client.ViewModel.Operation;

using ReactiveUI;

using System.Reactive.Disposables;

namespace Client.View.Operation
{
    /// <summary>
    /// TwoMatOperation.xaml 的交互逻辑
    /// </summary>
    public partial class TwoMatOperationView : ReactiveUserControl<TwoMatOperationViewModel>
    {
        public TwoMatOperationView()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.ImageItems, v => v.cbxImageItems1.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ImageItems, v => v.cbxImageItems2.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsOperaEnable, v => v.cbxOperaMethod.IsEnabled).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.OperaMethodItems, v => v.cbxOperaMethod.ItemsSource).DisposeWith(d);
                this.WhenAnyValue(x => x.cbxImageItems1.SelectedValue)
                    .WhereNotNull()
                    .BindTo(ViewModel, x => x.FirstImageSelectValue)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.cbxImageItems2.SelectedValue)
                    .WhereNotNull()
                    .BindTo(ViewModel, vm => vm.SecondImageSelectValue)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.cbxOperaMethod.SelectedValue)
                   .WhereNotNull()
                   .BindTo(ViewModel, vm => vm.OperaMethod)
                   .DisposeWith(d);
            });
        }
    }
}