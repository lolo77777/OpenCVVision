namespace Client.ViewModel.Operation;

[OperationInfo(8.3, "条纹结构光", MaterialDesignThemes.Wpf.PackIconKind.Video3d)]
public class GrayCodeViewModel : OperaViewModelBase
{
    public ReactiveCommand<Unit, Unit> View3dCommand { get; protected set; }

    protected override void SetupCommands()
    {
        base.SetupCommands();
        IScreen MainScreen = _resolver.GetService<IScreen>("MainHost");
        View3dCommand = ReactiveCommand.CreateFromObservable(() => MainScreen.Router.Navigate.Execute(_resolver.GetService<View3DViewModel>()).Select(_ => Unit.Default));
    }
}