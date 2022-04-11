namespace Client.ViewModel;

public class NavigationViewModel : ViewModelBase
{
    private IObservable<IList<Type>> _navigation;

    [Reactive] public IList<NaviItemStr> NaviItems { get; private set; } = new List<NaviItemStr>();
    [Reactive] public int NaviSelectItemIndex { get; private set; }

    private static NaviItemStr GetNaviItem(Type type)
    {
        (double id, string icon, string info) info = OpStaticMethod.GetOpInfo(type);
        return new NaviItemStr { Id = info.id, OperaPanelInfo = info.info, Icon = info.icon };
    }

    private static IList<NaviItemStr> SetItems(IList<Type> types)
    {
        return types.ToList().Select(t => GetNaviItem(t)).OrderBy(nit => nit.Id).ToList();
    }

    protected override void SetupStart()
    {
        base.SetupStart();
        string pathbase = AppDomain.CurrentDomain.BaseDirectory;
        string pathDll = $@"{pathbase}\Client.ViewModel.dll";
        IList<Type> types = Assembly.LoadFrom(pathDll).GetTypes().Where(t => t.IsSubclassOf(typeof(OperaViewModelBase))).ToList();
        MessageBus.Current.SendMessage(types);
    }

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        base.SetupSubscriptions(d);
        this.WhenAnyValue(x => x.NaviSelectItemIndex)
            .Where(ind => ind >= 0 && NaviItems.Count() > ind)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(ind => MessageBus.Current.SendMessage(NaviItems.ElementAt(ind)))
            .DisposeWith(d);
        MessageBus.Current.Listen<IList<Type>>()
            .WhereNotNull()
            .Select(vs => SetItems(vs))
            .BindTo(this, x => x.NaviItems)
            .DisposeWith(d);
    }
}

public class NaviItemStr
{
    public string Icon { get; set; }
    public double Id { get; set; }
    public string OperaPanelInfo { get; set; }
}