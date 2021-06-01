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
    /// ConnectedComponentsView.xaml 的交互逻辑
    /// </summary>
    public partial class ConnectedComponentsView : ReactiveUserControl<ConnectedComponentsViewModel>
    {
        public ConnectedComponentsView()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.AreaLimit, v => v.sliderAreaMax.Maximum).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.HeightLimit, v => v.sliderHeightMax.Maximum).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.WidthLimit, v => v.sliderWidthMax.Maximum).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AreaLimit, v => v.sliderAreaMin.Maximum).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.HeightLimit, v => v.sliderHeightMin.Maximum).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.WidthLimit, v => v.sliderWidthMin.Maximum).DisposeWith(d);

                this.WhenAnyValue(x => x.FilterList.SelectedItems)
                    .Select(t => (IEnumerable<object>)t)
                    .Where(listtmp => listtmp.Count() > 0)
                    .Select(listtmp => listtmp.Select(t => ((ListBoxItem)t).Content.ToString()))
                    .BindTo(ViewModel, x => x.Filters)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderAreaMax.Value)
                    .BindTo(ViewModel, x => x.AreaMax)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderAreaMin.Value)
                    .BindTo(ViewModel, x => x.AreaMin)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderHeightMax.Value)
                    .BindTo(ViewModel, x => x.HeightMax)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderHeightMin.Value)
                    .BindTo(ViewModel, x => x.HeightMin)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderWidthMin.Value)
                    .BindTo(ViewModel, x => x.WidthMin)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.sliderWidthMax.Value)
                    .BindTo(ViewModel, x => x.WidthMax)
                    .DisposeWith(d);
            });
        }
    }
}