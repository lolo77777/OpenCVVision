using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using DynamicData;

using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Client.Model.Service;
using Splat;
using Client.Model.Entity;
using DynamicData.Tests;
using System.Reactive;

namespace Client.ViewModel
{
    public class HistoryItem
    {
        public Guid HistoryItemId { get; set; }
        public WriteableBitmap HistoryItemImg { get; set; }
        public string HistoryItemTxtMark { get; set; }
    }

    public class ImageViewModel : ReactiveObject
    {
        private ReadOnlyObservableCollection<HistoryItem> _historyItems;
        private SourceCache<HistoryItem, Guid> _historyItemsTmp = new(t => t.HistoryItemId);
        private IImageDataManager _imageDataManager;
        private IReadonlyDependencyResolver _resolver = Locator.Current;
        private ResourcesTracker _rt = new ResourcesTracker();
        public ReactiveCommand<Unit, Unit> AddOutputImgToImgManagerCommand { get; set; }
        public ReadOnlyObservableCollection<HistoryItem> HistoryItems => _historyItems;
        [Reactive] public int HistoryItemSelectInd { get; set; }
        public WriteableBitmap InputImg { [ObservableAsProperty] get; }
        [Reactive] public string OutputImageMarkTxt { get; set; }
        public WriteableBitmap OutputImg { [ObservableAsProperty]get; }
        public ReactiveCommand<Unit, bool> RemoveImgFromImgManagerCommand { get; set; }
        [Reactive] public string Time { get; private set; }

        public ImageViewModel(IImageDataManager imageDataManager = null)
        {
            _imageDataManager = imageDataManager ?? _resolver.GetService<IImageDataManager>();
            SetupCommands();
            SetupSubscriptions();
        }

        private HistoryItem ConvertData(ImageData imageData)
        {
            var wtBitmap = MatResizeWt(imageData.ImageMat);

            return new HistoryItem { HistoryItemId = imageData.ImageId, HistoryItemTxtMark = imageData.TxtMarker, HistoryItemImg = wtBitmap };
        }

        private void Init()
        {
        }

        private WriteableBitmap MatResizeWt(Mat mat)
        {
            var scaleY = 90d / mat.Height;
            var dst = _rt.T(mat.Resize(Size.Zero, scaleY, scaleY));
            return dst.ToWriteableBitmap();
        }

        private void SetupCommands()
        {
            AddOutputImgToImgManagerCommand = ReactiveCommand.Create(() => _imageDataManager.AddOutputImage(OutputImageMarkTxt));
            RemoveImgFromImgManagerCommand = ReactiveCommand.Create(() => _imageDataManager.RemoveCurrentImage());
        }

        private void SetupSubscriptions()
        {
            _imageDataManager.SourceCacheImageData
               .Connect()
               .ObserveOn(RxApp.MainThreadScheduler)
               .Transform(it => ConvertData(it))
               .Do(it => UpdateHistoryItems(it))
               .Subscribe();
            _historyItemsTmp
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _historyItems)
                .Subscribe();
            this.WhenAnyValue(x => x.HistoryItemSelectInd)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(i => i >= 0 && HistoryItems.Count > i)
                .Select(i => HistoryItems.ElementAt(i).HistoryItemId)
                .Do(guid => _imageDataManager.InputMatGuidSubject.OnNext(guid))
                .Subscribe();
            _imageDataManager.InputMatGuidSubject
                .WhereNotNull()
                .Select(guid => _imageDataManager.GetImage(guid).ImageMat.ToWriteableBitmap())
                .ToPropertyEx(this, x => x.InputImg);

            _imageDataManager.OutputMatSubject
                .ObserveOn(RxApp.MainThreadScheduler)
                .WhereNotNull()
                .Select(mat => mat.ToWriteableBitmap())
                .ToPropertyEx(this, x => x.OutputImg);
            MessageBus.Current.Listen<double>("Time")
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(d => $"{d}ms")
                .BindTo(this, x => x.Time);
        }

        private void UpdateHistoryItems(IChangeSet<HistoryItem, Guid> changes)
        {
            var items = changes.Select(t => t.Current).ToList();
            if (changes.Adds > 0 || changes.Updates > 0)
            {
                _historyItemsTmp.AddOrUpdate(items);
            }
            else if (changes.Removes > 0)
            {
                _historyItemsTmp.Remove(items);
            }
        }
    }
}