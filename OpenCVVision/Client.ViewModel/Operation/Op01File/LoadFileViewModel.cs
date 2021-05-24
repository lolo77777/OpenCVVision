using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Client.Common;
using Client.Model.Service;

using OpenCvSharp;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Splat;

namespace Client.ViewModel.Operation.Op01File
{
    [OperationInfo("加载图片")]
    public class LoadFileViewModel : ReactiveObject, IOperationViewModel
    {
        private IImageDataManager _imageDataManager;
        private IReadonlyDependencyResolver _resolver = Locator.Current;
        public IScreen HostScreen { get; }
        public ReactiveCommand<Unit, Unit> LoadImageCommand { get; private set; }
        [Reactive] public string TxtImageFilePath { get; set; }
        public string UrlPathSegment { get; }

        public LoadFileViewModel(IImageDataManager imageDataManager = null)
        {
            _imageDataManager = imageDataManager ?? _resolver.GetService<IImageDataManager>();
            LoadImageCommand = ReactiveCommand.Create(LoadFile);
            this.WhenAnyValue(x => x.TxtImageFilePath)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .Where(str => !string.IsNullOrWhiteSpace(str))
                .Select(str => Cv2.ImRead(str))
                .Do(mat => _imageDataManager.AddImage("Src", mat))
                .Do(mat => _imageDataManager.OutputMat.OnNext(mat))
                .Subscribe();
        }

        #region PrivateFunction

        private void LoadFile()
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Image files (*.jpg)|*.jpg|(*.bmp)|*.bmp|(*.png)|*.png";
            var result = openFileDialog.ShowDialog();
            if (result.Equals(DialogResult.OK))
            {
                TxtImageFilePath = openFileDialog.FileName;
            }
            else
            {
                TxtImageFilePath = string.Empty;
            }
        }

        #endregion PrivateFunction
    }
}