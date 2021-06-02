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

using Client.ViewModel.Operation;

using ReactiveUI;

namespace Client.View.Operation
{
    /// <summary>
    /// ContoursView.xaml 的交互逻辑
    /// </summary>
    public partial class ContoursView : ReactiveUserControl<ContoursViewModel>
    {
        public ContoursView()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.RetrievalModesStr, v => v.cbxRetrievalModes.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ContourApproximationModesStr, v => v.cbxContourApproximationModes.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ContourIdItems, v => v.cbxContourIdItems.ItemsSource).DisposeWith(d);
                this.WhenAnyValue(x => x.cbxRetrievalModes.SelectedValue)
                    .WhereNotNull()
                    .BindTo(ViewModel, x => x.RetrievalSelectValue)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.cbxContourApproximationModes.SelectedValue)
                    .WhereNotNull()
                    .BindTo(ViewModel, x => x.ContourApproximationSelectValue)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.cbxContourIdItems.SelectedValue)
                    .BindTo(ViewModel, x => x.ContourIdItemSelectValue)
                    .DisposeWith(d);
            });
        }
    }
}