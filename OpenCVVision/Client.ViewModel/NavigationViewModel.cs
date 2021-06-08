using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Client.Common;
using Client.ViewModel.Operation;

using DynamicData;

using MaterialDesignThemes.Wpf;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Client.ViewModel
{
    public class NavigationViewModel : ReactiveObject
    {
        private SourceList<Type> OpVMTypes = new SourceList<Type>();
        [Reactive] public IEnumerable<NaviItem> NaviItems { get; private set; } = new List<NaviItem>();
        [Reactive] public int NaviSelectItemIndex { get; private set; }

        public NavigationViewModel()
        {
            SetupSubscriptions();
            SetupStart();
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

        private void SetupStart()
        {
            var pathbase = AppDomain.CurrentDomain.BaseDirectory;
            var dllpath = $@"{pathbase}\Client.ViewModel.dll";
            var types = Assembly.LoadFrom(dllpath).GetTypes().Where(t => t.IsSubclassOf(typeof(OperaViewModelBase)));
            MessageBus.Current.SendMessage(types);
        }

        private void SetupSubscriptions()
        {
            this.WhenAnyValue(x => x.NaviSelectItemIndex)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(ind => ind >= 0 && NaviItems.Count() > ind)
                .Do(ind => MessageBus.Current.SendMessage(NaviItems.ElementAt(ind)))
                .Subscribe();

            MessageBus.Current.Listen<IEnumerable<Type>>()
                .WhereNotNull()
                .Select(vs => SetItems(vs))
                .BindTo(this, x => x.NaviItems);
        }
    }

    public class NaviItem
    {
        public PackIconKind Icon { get; set; }
        public double Id { get; set; }
        public string OperaPanelInfo { get; set; }
    }
}