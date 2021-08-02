using Client.ViewModel;

using MaterialDesignThemes.Wpf;

using Splat;

using System.Diagnostics;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly PaletteHelper _paletteHelper = new();
        private readonly IReadonlyDependencyResolver _resolver = Locator.Current;
        private ITheme _theme;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new AppBootstrapper();


        }

        private void OpenGitSite_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "https://gitee.com/lolo77/OpenCVVision");
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