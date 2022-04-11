namespace Client.ViewModel.Operation;

[OperationInfo(8.4, "光度立体法", "Lightbulb")]
public class PhotometricStereoViewModel : OperaViewModelBase
{
    public ReactiveCommand<Unit, Unit> ViewPhotometricCommand { get; protected set; }

    protected override void SetupSubscriptions(CompositeDisposable d)
    {
        base.SetupSubscriptions(d);
    }

    protected override void SetupCommands()
    {
        base.SetupCommands();
        IScreen MainScreen = _resolver.GetService<IScreen>("MainHost");
        ViewPhotometricCommand = ReactiveCommand.CreateFromObservable(() =>
        MainScreen.Router.Navigate.Execute(_resolver.GetService<ViewPhotometricViewModel>())
        .Select(_ => Unit.Default));
    }
}