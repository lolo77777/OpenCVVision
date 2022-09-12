using Client.Services;

namespace Client.ViewModel
{
    /*
    ------------rxui介绍-------------
    System.Reactive简称rx.net，是响应式编程的.net实现；
    ReactiveUI简称rxui；是基于响应式编程的UI扩展；
    rxui以跨平台为主要目标，支持wpf,winform,upw,winui,maui,Xamarin,Blazor,Uno,Avalonia
    基于跨平台的目标，rxui大多方法都以扩展方法提供，依赖注入组件、事件总线、线程调度器等以静态类的方式出现；从MVVM框架的角度来看，这是与其他框架区别较大的地方
    为了兼容多平台，大多数功能的默认提供尽量以简单轻量为主；比如splat组件附带的依赖注入，日志等，同样支持非常方便的一键式扩展其他ioc，loger组件等
    rxui是viewmodel first类型的mvvm框架，因此其中的视图导航等都通过viewmodel的操作来实现

    ------------项目介绍-------------
    解决方案包括以下5个项目：
    common，通用的基类或方法类
    data，包含在项目中的文件、图片等资源
    model，服务、实体类等文件
    view，xaml视图部分
    viewmodel,视图对应的viewmodel部分。

    ------------其   他-------------
    每个项目中包含自己register注册类，在启动时，通过反射来实现每个项目中需要的IOC注册
    界面的响应式管道配置处理说明参考ViewModelBase.cs文件里的注释

    ------------OpencvSharp-------------
    opencvsharp图像处理的逻辑分布在ViewModel项目的Operation文件夹里
    大部分处理都在UpdateOutput方法里实现，通过对界面参数变化的订阅，自动以UpdateOutput方法处理并显示
    */

    /// <summary>
    /// 程序引导部分
    /// </summary>
    public class AppBootstrapper : ReactiveObject, IScreen, IEnableLogger
    {
        public RoutingState Router { get; private set; } = new RoutingState();

        public AppBootstrapper()
        {
            //默认的异常处理方式设置
            RxApp.DefaultExceptionHandler = new MyCoolObservableExceptionHandler();
            RxApp.SuppressViewCommandBindingMessage = true;

            Locator.CurrentMutable.RegisterLazySingleton<Splat.ILogger>(() => new ObservableLogger());

            LoadDlls();

            this.Log().Info("*******************");
            this.Log().Info("******程序启动******");
            this.Log().Info("*******************");

            //窗体Screen名称注册
            Locator.CurrentMutable.RegisterConstant<IScreen>(this, "MainHost");
            //rxui是iewmoedl first模式，导航至主窗体ViewModel，以显示对应的view
            Router.Navigate.Execute(Locator.Current.GetService<ShellViewModel>());
        }

        /// <summary>
        /// 反射加载模块,通过每个项目的RegisterBase子类进行注册
        /// </summary>
        private void LoadDlls()
        {
            string starupPath = Environment.CurrentDirectory;
            List<string> dlls = Directory.GetFiles(starupPath).Where(f => f.Contains(".dll")).Select(f => f.Replace(starupPath + @"\", "")).Where(n => n.Contains("Client")).ToList();
            foreach (string dll in dlls)
            {
                List<object> tmp = (
                    from t in Assembly.LoadFrom(dll).GetTypes()
                    where t.IsSubclassOf(typeof(RegisterBase))
                    select Activator.CreateInstance(t)).ToList();
                tmp.Clear();
            }
        }
    }
}