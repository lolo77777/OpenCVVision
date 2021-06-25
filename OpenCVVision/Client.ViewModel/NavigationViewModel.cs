using MaterialDesignThemes.Wpf;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;

namespace Client.ViewModel
{
    public class NavigationViewModel : ViewModelBase
    {
        [Reactive] public IEnumerable<NaviItem> NaviItems { get; private set; } = new List<NaviItem>();
        [Reactive] public int NaviSelectItemIndex { get; private set; }

        public NavigationViewModel()
        {
            //SetupSubscriptions();
            //SetupStart();
        }

        private NaviItem GetNaviItem(Type type)
        {
            var info = OpStaticMethod.GetOpInfo(type);
            return new NaviItem { Id = info.id, OperaPanelInfo = info.info, Icon = info.icon };
        }

        private IEnumerable<NaviItem> SetItems(IEnumerable<Type> types)
        {
            return types.ToList().Select(t => GetNaviItem(t)).OrderBy(nit => nit.Id);
        }

        protected override void SetupStart()
        {
            base.SetupStart();
            var pathbase = AppDomain.CurrentDomain.BaseDirectory;
            var dllpath = $@"{pathbase}\Client.ViewModel.dll";
            var types = Assembly.LoadFrom(dllpath).GetTypes().Where(t => t.IsSubclassOf(typeof(OperaViewModelBase)));
            MessageBus.Current.SendMessage(types);
        }
        protected override void SetupSubscriptions(CompositeDisposable d)
        {
            base.SetupSubscriptions(d);
            this.WhenAnyValue(x => x.NaviSelectItemIndex)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(ind => ind >= 0 && NaviItems.Count() > ind)
                .Do(ind => MessageBus.Current.SendMessage(NaviItems.ElementAt(ind)))
                .Subscribe()
                .DisposeWith(d);

            MessageBus.Current.Listen<IEnumerable<Type>>()
                .WhereNotNull()
                .Select(vs => SetItems(vs))
                .BindTo(this, x => x.NaviItems)
                .DisposeWith(d);
        }
    }

    public class NaviItem
    {
        public PackIconKind Icon { get; set; }
        public double Id { get; set; }
        public string OperaPanelInfo { get; set; }
    }
}