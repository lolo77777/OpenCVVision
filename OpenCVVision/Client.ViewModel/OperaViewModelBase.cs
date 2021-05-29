using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
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
        protected ResourcesTracker _rt = new ResourcesTracker();
        protected Mat _sigleSrc;
        protected Mat _src;
        protected bool IsRun = false;
        public ViewModelActivator Activator { get; }
        [Reactive] public bool CanOperat { get; set; }
        public IScreen HostScreen { get; }
        public string UrlPathSegment { get; }

        public OperaViewModelBase(IImageDataManager imageDataManager = null)
        {
            Activator = new ViewModelActivator();
            _imageDataManager = imageDataManager ?? _resolver.GetService<IImageDataManager>();
            _imageDataManager.InputMatGuidSubject
              .Select(guid => guid != null)
              .BindTo(this, x => x.CanOperat);
        }

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
    }
}