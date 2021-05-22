using System;
using System.Collections.Generic;
using System.Linq;
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
                var list = new List<PackIconKind>();
                list.Add(PackIconKind.Abc);
                list.Add(PackIconKind.AbugidaDevanagari);
                NavitionItems.ItemsSource = list;
            });
        }
    }
}