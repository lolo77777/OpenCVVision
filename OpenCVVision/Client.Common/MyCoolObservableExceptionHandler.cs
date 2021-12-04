namespace Client.Common
{
    public class MyCoolObservableExceptionHandler : IObserver<Exception>,IEnableLogger
    {
        public void OnNext(Exception value)
        {
            if (Debugger.IsAttached) Debugger.Break();
            RxApp.MainThreadScheduler.Schedule(() =>  this.Log().Fatal(value,value.Message) );
        }

        public void OnError(Exception error)
        {
            if (Debugger.IsAttached) Debugger.Break();
            RxApp.MainThreadScheduler.Schedule(() => this.Log().Error(error, error.Message));
        }

        public void OnCompleted()
        {

        }
    }
}