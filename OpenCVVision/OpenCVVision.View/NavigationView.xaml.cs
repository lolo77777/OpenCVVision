

namespace OpenCVVision.View;

[SingleInstanceView]
public partial class NavigationView
{
    private readonly DoubleAnimation withAnimation1 = new();
    private readonly DoubleAnimation withAnimation2 = new();

    public NavigationView()
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
            this.OneWayBind(ViewModel, vm => vm.NaviItems, v => v.NavigationTab.ItemsSource, CvtNaviItem).DisposeWith(d);
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

    private IEnumerable<NaviItemIcon> CvtNaviItem(IEnumerable<NaviItemStr> src)
    {
        return src.ToList().Select(s => new NaviItemIcon { Icon = Enum.Parse<PackIconKind>(s.Icon), Id = s.Id, OperaPanelInfo = s.OperaPanelInfo });
    }
}

public class NaviItemIcon
{
    public PackIconKind Icon { get; set; }
    public double Id { get; set; }
    public string OperaPanelInfo { get; set; }
}