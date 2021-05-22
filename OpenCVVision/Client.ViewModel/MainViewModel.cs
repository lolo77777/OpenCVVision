using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Splat;

namespace Client.ViewModel
{
    public class MainViewModel : ReactiveObject
    {
        private readonly IReadonlyDependencyResolver _resolver = Locator.Current;
        public NavigationViewModel NavigationViewModelSam { get; private set; }
        [Reactive] public string TxtUi { get; set; } = "hello";

        public MainViewModel(NavigationViewModel navigationViewModel = null)
        {
            navigationViewModel ??= _resolver.GetService<NavigationViewModel>();
            NavigationViewModelSam = navigationViewModel;
        }
    }
}