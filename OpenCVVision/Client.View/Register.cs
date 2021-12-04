namespace Client.View;

internal class Register : RegisterBase
{
    //private void RegisterOperationView<T1, T2>() where T1 : OperaViewModelBase where T2 : IViewFor<T1>, new()
    //{
    //    _mutable.Register<IViewFor<T1>>(() => new T2());
    //}

    public override void ConfigService()
    {
        // Locator.CurrentMutable.RegisterLazySingleton(() => new ConventionalViewLocator(), typeof(IViewLocator));
        //通过反射IViewFor接口，自动注册IViewFor<viewmodel>与对应的view视图
        Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// 也可以注册视图定位器，设定通过viewmodel寻找视图的规则，通过视图定位器提供对应的视图
    /// </summary>
    public class ConventionalViewLocator : IViewLocator
    {
        public IViewFor ResolveView<T>(T? viewModel, string contract = null)
        {
            string viewModelName = viewModel.GetType().FullName;
            string viewTypeName = viewModelName.Replace("ViewModel", "View");

            try
            {
                Type viewType = Type.GetType(viewTypeName);
                if (viewType == null)
                {
                    this.Log().Error($"找不到与{viewModelName}对应的{viewTypeName}.");
                    return null;
                }
                return Activator.CreateInstance(viewType) as IViewFor;
            }
            catch (Exception)
            {
                this.Log().Error($"无法实例化{viewTypeName}.");
                throw;
            }
        }
    }
}