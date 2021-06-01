using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Common;
using Client.ViewModel.Operation;
using Client.ViewModel.Operation.Op01File;
using Client.ViewModel.Operation.Op02ColorSpace;
using Client.ViewModel.Operation.Op03PreProcessing;

using MaterialDesignThemes.Wpf;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Client.ViewModel
{
    public class NavigationViewModel : ReactiveObject
    {
        [Reactive] public IEnumerable<NaviItem> NaviItems { get; private set; } = new List<NaviItem>();
        [Reactive] public int NaviSelectItemIndex { get; private set; }

        public NavigationViewModel()
        {
            //NaviItems = SetItems();

            this.WhenAnyValue(x => x.NaviSelectItemIndex)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(ind => ind >= 0 && NaviItems.Count() > ind)
                .Do(ind => MessageBus.Current.SendMessage(NaviItems.ElementAt(ind)))
                .Subscribe();

            Observable
                .Start(() => SetItems())

                .ObserveOn(RxApp.MainThreadScheduler)
                .BindTo(this, x => x.NaviItems);
        }

        private IEnumerable<NaviItem> SetItems()
        {
            List<NaviItem> listtmp = new();
            listtmp.Add(new NaviItem { Icon = PackIconKind.File, OperaPanelInfo = StaticMethod.GetInfo<LoadFileViewModel>() });
            listtmp.Add(new NaviItem { Icon = PackIconKind.Color, OperaPanelInfo = StaticMethod.GetInfo<ColorSpaceViewModel>() });
            listtmp.Add(new NaviItem { Icon = PackIconKind.Filter, OperaPanelInfo = StaticMethod.GetInfo<FilterViewModel>() });
            listtmp.Add(new NaviItem { Icon = PackIconKind.NumericZero, OperaPanelInfo = StaticMethod.GetInfo<ThreshouldViewModel>() });
            listtmp.Add(new NaviItem { Icon = PackIconKind.MortarPestle, OperaPanelInfo = StaticMethod.GetInfo<MorphologyViewModel>() });
            listtmp.Add(new NaviItem { Icon = PackIconKind.Connection, OperaPanelInfo = StaticMethod.GetInfo<ConnectedComponentsViewModel>() });
            listtmp.Add(new NaviItem { Icon = PackIconKind.Circle, OperaPanelInfo = StaticMethod.GetInfo<ContoursViewModel>() });
            return listtmp;
        }
    }

    public class NaviItem
    {
        public PackIconKind Icon { get; set; }
        public string OperaPanelInfo { get; set; }
    }
}