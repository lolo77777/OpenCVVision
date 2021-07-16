using System.Reactive.Disposables;

namespace Client.ViewModel.Operation
{
    [OperationInfo(8.4, "光度立体法", MaterialDesignThemes.Wpf.PackIconKind.Lightbulb)]
    public class PhotometricStereoViewModel : OperaViewModelBase
    {
        protected override void SetupSubscriptions(CompositeDisposable d)
        {
            base.SetupSubscriptions(d);
        }
    }
}
