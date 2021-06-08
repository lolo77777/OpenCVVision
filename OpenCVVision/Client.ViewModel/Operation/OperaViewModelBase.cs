using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Model.Service;

using OpenCvSharp;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Splat;

namespace Client.ViewModel
{
    public class OperaViewModelBase : ReactiveObject, IOperationViewModel, IActivatableViewModel
    {
        protected IImageDataManager _imageDataManager;
        protected IReadonlyDependencyResolver _resolver = Locator.Current;

        /// <summary>
        /// mat标记，方便释放
        /// </summary>
        protected ResourcesTracker _rt = new ResourcesTracker();

        protected Mat _sigleSrc;
        protected Mat _src;

        /// <summary>
        /// 标记操作进行状态
        /// </summary>
        protected bool IsRun = false;

        public ViewModelActivator Activator { get; }

        /// <summary>
        /// 标记能否进行操作
        /// </summary>
        [Reactive] public bool CanOperat { get; set; }

        public IScreen HostScreen { get; }
        public string UrlPathSegment { get; }

        public OperaViewModelBase(IImageDataManager imageDataManager = null)
        {
            Activator = new ViewModelActivator();
            _imageDataManager = imageDataManager ?? _resolver.GetService<IImageDataManager>();
            this.WhenActivated(d =>
            {
                SetupStart(d);
                SetupCommands(d);
                SetupSubscriptions(d);
            });
        }

        /// <summary>
        /// 执行操作，发送操作执行的时间
        /// </summary>
        /// <param name="action">图像的操作</param>
        protected void SendTime(Action action)
        {
            if (!IsRun)
            {
                IsRun = true;
                var t1 = Cv2.GetTickCount();
                _src = _rt.T(_imageDataManager.GetCurrentMat().Clone());
                _sigleSrc = _rt.T(_src.Channels() > 1 ? _src.CvtColor(ColorConversionCodes.BGR2GRAY) : _src);
                action.Invoke();
                _rt.Dispose();
                var t2 = Cv2.GetTickCount();
                var t = Math.Round((t2 - t1) / Cv2.GetTickFrequency() * 1000, 0);
                MessageBus.Current.SendMessage(t, "Time");
                IsRun = false;
            }
        }

        /// <summary>
        /// 设置命令
        /// </summary>
        protected virtual void SetupCommands(CompositeDisposable d)
        {
        }

        /// <summary>
        /// 设置启动时加载
        /// </summary>
        protected virtual void SetupStart(CompositeDisposable d)
        {
        }

        /// <summary>
        /// 设置流订阅
        /// </summary>
        protected virtual void SetupSubscriptions(CompositeDisposable d)
        {
            _imageDataManager.InputMatGuidSubject
              .Select(guid => guid != null)
              .BindTo(this, x => x.CanOperat)
              .DisposeWith(d);
        }
    }
}