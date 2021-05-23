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
using System.Windows.Media.Animation;
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
        private DoubleAnimation withAnimation1 = new();
        private DoubleAnimation withAnimation2 = new();

        public Navigation()
        {
            InitializeComponent();

            LoadAnimation();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.NaviItems, v => v.NavigationTab.ItemsSource).DisposeWith(d);
                this.WhenAnyValue(x => x.NavigationTab.SelectedIndex)
                    .BindTo(ViewModel, x => x.NaviSelectItemIndex)
                    .DisposeWith(d);
                this.WhenAnyValue(x => x.HumToggle.IsChecked)
                    .WhereNotNull()
                    .Do(b =>
                    {
                        if (!b.Value)
                        {
                            GridMain.BeginAnimation(WidthProperty, withAnimation1);
                        }
                        else
                        {
                            GridMain.BeginAnimation(WidthProperty, withAnimation2);
                        }
                    })
                    .Subscribe();
            });
        }

        private void LoadAnimation()
        {
            withAnimation1.From = 140;
            withAnimation1.To = 41;
            withAnimation1.Duration = TimeSpan.FromMilliseconds(260);
            withAnimation2.From = 41;
            withAnimation2.To = 140;
            withAnimation2.Duration = TimeSpan.FromMilliseconds(260);
        }
    }
}