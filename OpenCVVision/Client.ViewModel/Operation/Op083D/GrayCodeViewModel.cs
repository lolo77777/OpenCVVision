using ReactiveUI;

using Splat;

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Client.ViewModel.Operation
{
    [OperationInfo(8.3, "条纹结构光", MaterialDesignThemes.Wpf.PackIconKind.Video3d)]
    public class GrayCodeViewModel : OperaViewModelBase
    {
        private readonly IReadonlyDependencyResolver _resolver = Locator.Current;
        public ReactiveCommand<Unit, Unit> View3dCommand { get; protected set; }
        protected override void SetupCommands(CompositeDisposable d)
        {
            base.SetupCommands(d);
            var MainScreen = _resolver.GetService<IScreen>("MainHost");
            View3dCommand = ReactiveCommand.CreateFromTask(async () =>await MainScreen.Router.Navigate.Execute(_resolver.GetService<View3DViewModel>()).Select(_=>Unit.Default));
        }
    }
}