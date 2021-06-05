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

using ReactiveUI;

using Splat;

using Client.ViewModel;
using System.Diagnostics;
using MaterialDesignThemes.Wpf;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IViewFor<MainViewModel>
    {
        private PaletteHelper _paletteHelper = new();
        private IReadonlyDependencyResolver _resolver = Locator.Current;
        private ITheme _theme;

        #region ViewModel

        public static readonly DependencyProperty ViewModelProperty =
                  DependencyProperty.Register(
                      "ViewModel",
                      typeof(MainViewModel),
                      typeof(ReactiveWindow<MainViewModel>),
                      new PropertyMetadata(null));

        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }

        #endregion ViewModel

        public MainWindow(MainViewModel mainViewModel = null)
        {
            InitializeComponent();
            ViewModel = mainViewModel ?? _resolver.GetService<MainViewModel>();
            SetupBinding();
        }

        private void OpenGitSite_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "https://gitee.com/lolo77/OpenCVVision");
        }

        private void SetupBinding()
        {
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.NavigationViewModelSam, v => v.Nagivate.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ImageVMSam, v => v.ImgViewer.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Router, v => v.OperaPanel.Router).DisposeWith(d);
            });
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _theme = _theme ?? _paletteHelper.GetTheme();
            if (TBtnTheme.IsChecked.HasValue && TBtnTheme.IsChecked.Value)
            {
                _theme.SetBaseTheme(Theme.Dark);

                _paletteHelper.SetTheme(_theme);
            }
            else
            {
                _theme.SetBaseTheme(Theme.Light);

                _paletteHelper.SetTheme(_theme);
            }
        }
    }
}