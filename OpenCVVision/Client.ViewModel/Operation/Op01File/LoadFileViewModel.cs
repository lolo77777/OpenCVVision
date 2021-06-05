using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Common;
using Client.Model.Service;

using OpenCvSharp;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Splat;

namespace Client.ViewModel.Operation.Op01File
{
    [OperationInfo("加载图片")]
    public class LoadFileViewModel : OperaViewModelBase
    {
        private Interaction<Unit, string> _loadFileConfirm = new();
        public Interaction<Unit, string> LoadFileConfirm => _loadFileConfirm;
        public ReactiveCommand<Unit, Unit> LoadImageCommand { get; private set; }

        [Reactive] public string TxtImageFilePath { get; set; }

        public LoadFileViewModel()
        {
        }

        protected override void SetupCommands(CompositeDisposable d)
        {
            base.SetupCommands(d);
            LoadImageCommand = ReactiveCommand.Create(LoadFile);
        }

        protected override void SetupSubscriptions(CompositeDisposable d)
        {
            base.SetupSubscriptions(d);
            this.WhenAnyValue(x => x.TxtImageFilePath)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .Where(str => !string.IsNullOrWhiteSpace(str))
                .Select(str => Cv2.ImRead(str))
                .Do(mat => _imageDataManager.AddImage("Src", mat))
                .Do(mat => _imageDataManager.OutputMatSubject.OnNext(mat))
                .Subscribe();
        }

        #region PrivateFunction

        private void LoadFile()
        {
            _loadFileConfirm.Handle(Unit.Default)
               .Subscribe(str => TxtImageFilePath = str);
        }

        #endregion PrivateFunction
    }
}