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
    public class OperaViewModelBase : ReactiveObject, IOperationViewModel
    {
        protected IImageDataManager _imageDataManager;
        protected IReadonlyDependencyResolver _resolver = Locator.Current;
        protected ResourcesTracker _rt = new ResourcesTracker();
        protected Mat _src;
        [Reactive] public bool CanOperat { get; set; }
        public IScreen HostScreen { get; }
        public string UrlPathSegment { get; }

        public OperaViewModelBase(IImageDataManager imageDataManager = null)
        {
            _imageDataManager = imageDataManager ?? _resolver.GetService<IImageDataManager>();
            _imageDataManager.InputMatGuidSubject
                .Select(guid => guid != null)
                .BindTo(this, x => x.CanOperat);
        }
    }
}