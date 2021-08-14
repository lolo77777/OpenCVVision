using Client.Common;

using ReactiveUI;

using Splat;

using System;
using System.Reflection;

namespace Client.View
{
    internal class Register : RegisterBase
    {
        //private void RegisterOperationView<T1, T2>() where T1 : OperaViewModelBase where T2 : IViewFor<T1>, new()
        //{
        //    _mutable.Register<IViewFor<T1>>(() => new T2());
        //}

        public override void ConfigService()
        {

           // Locator.CurrentMutable.RegisterLazySingleton(() => new ConventionalViewLocator(), typeof(IViewLocator));
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
        }
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
}
