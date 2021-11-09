namespace Client.Common
{
    public abstract class RegisterBase
    {
        protected IMutableDependencyResolver _mutable = Locator.CurrentMutable;
        protected IReadonlyDependencyResolver _resolver = Locator.Current;

        public RegisterBase()
        {
            ConfigService();
        }

        public abstract void ConfigService();
    }
}