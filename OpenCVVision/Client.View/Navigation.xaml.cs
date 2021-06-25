using Client.ViewModel;

using ReactiveUI;

using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Media.Animation;

namespace Client.View
{
    /// <summary>
    /// Navigation.xaml 的交互逻辑
    /// </summary>
    public partial class Navigation : ReactiveUserControl<NavigationViewModel>
    {
        private readonly DoubleAnimation withAnimation1 = new();
        private readonly DoubleAnimation withAnimation2 = new();

        public Navigation()
        {
            InitializeComponent();

            LoadAnimation();
            SetupBinding();
        }

        private void LoadAnimation()
        {
            withAnimation1.From = 140;
            withAnimation1.To = 41;
            withAnimation1.Duration = TimeSpan.FromMilliseconds(120);
            withAnimation2.From = 41;
            withAnimation2.To = 140;
            withAnimation2.Duration = TimeSpan.FromMilliseconds(120);
        }

        private void SetupBinding()
        {
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
                    .Subscribe()
                    .DisposeWith(d);
            });
        }
    }
}