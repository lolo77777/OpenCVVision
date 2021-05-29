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
                this.WhenAnyValue(x => x.FilterList.SelectedItems)
                    .Select(t => (IEnumerable<ListBoxItem>)t)
                    .Where(listtmp => listtmp.Count() > 0)
                    .SelectMany(listtmp => listtmp.Select(t => t.Content))
                    .BindTo(ViewModel, x => x.Filters)
                    .DisposeWith(d);
            });
        }
    }
}