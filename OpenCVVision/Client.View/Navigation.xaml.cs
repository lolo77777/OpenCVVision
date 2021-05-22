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

using MaterialDesignThemes.Wpf;

using ReactiveUI;

namespace Client.View
{
    /// <summary>
    /// Navigation.xaml 的交互逻辑
    /// </summary>
    public partial class Navigation : ReactiveUserControl<NavigationViewModel>
    {
        public Navigation()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.NaviItems, v => v.NavitionItems.ItemsSource).DisposeWith(d);
                this.WhenAnyValue(x => x.NavitionItems.SelectedIndex)
                    .BindTo(ViewModel, x => x.NaviSelectItemIndex)
                    .DisposeWith(d);
            });
        }
    }
}