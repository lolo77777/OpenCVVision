using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

using MaterialDesignThemes.Wpf;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Client.ViewModel
{
    public class NavigationViewModel : ReactiveObject
    {
        [Reactive] public IEnumerable<NaviItem> NaviItems { get; private set; }
        [Reactive] public int NaviSelectItemIndex { get; private set; }

        public NavigationViewModel()
        {
            NaviItems = SetItems();
            this.WhenAnyValue(x => x.NaviSelectItemIndex)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(ind => ind >= 0)
                .Do(ind => MessageBus.Current.SendMessage(NaviItems.ElementAt(ind)))
                .Subscribe();
        }

        private IEnumerable<NaviItem> SetItems()
        {
            List<NaviItem> listtmp = new();
            listtmp.Add(new NaviItem { Icon = PackIconKind.FileLocation, TxtTooltip = "加载图片" });
            listtmp.Add(new NaviItem { Icon = PackIconKind.UserConvert, TxtTooltip = "转换图像" });
            return listtmp;
        }
    }

    public class NaviItem
    {
        public PackIconKind Icon { get; set; }
        public string TxtTooltip { get; set; }
    }
}