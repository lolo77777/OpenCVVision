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

            SetupSubscriptions();
        }

        private NaviItem GetNaviItem<T>()
        {
            var info = OpStaticMethod.GetOpInfo<T>();
            return new NaviItem { OperaPanelInfo = info.info, Icon = info.icon };
        }

        private IEnumerable<NaviItem> SetItems()
        {
            List<NaviItem> listtmp = new();
            listtmp.Add(GetNaviItem<LoadFileViewModel>());
            listtmp.Add(GetNaviItem<RoiViewModel>());
            listtmp.Add(GetNaviItem<ColorSpaceViewModel>());
            listtmp.Add(GetNaviItem<FilterViewModel>());
            listtmp.Add(GetNaviItem<ThreshouldViewModel>());
            listtmp.Add(GetNaviItem<MorphologyViewModel>());
            listtmp.Add(GetNaviItem<ConnectedComponentsViewModel>());
            listtmp.Add(GetNaviItem<ContoursViewModel>());
            listtmp.Add(GetNaviItem<LaserLineViewModel>());
            return listtmp;
        }

        private void SetupSubscriptions()
        {
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
    }

    public class NaviItem
    {
        public PackIconKind Icon { get; set; }
        public string OperaPanelInfo { get; set; }
    }
}