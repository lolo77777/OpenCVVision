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
        public ReadOnlyObservableCollection<HistoryItem> HistoryItems => _historyItems;
        [Reactive] public int HistoryItemSelectInd { get; set; }
        public WriteableBitmap InputImg { [ObservableAsProperty] get; }
        public WriteableBitmap OutputImg { [ObservableAsProperty]get; }

        public ImageViewModel(IImageDataManager imageDataManager = null)
        {
            _imageDataManager = imageDataManager ?? _resolver.GetService<IImageDataManager>();
            _imageDataManager.SourceCacheImageData
                             .Connect()
                             .ObserveOn(RxApp.MainThreadScheduler)
                             .Transform(it => ConvertData(it))
                             .Do(it => UpdateHistoryItems(it))
                             .Subscribe();
            _historyItemsTmp.Connect()
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Bind(out _historyItems)
                            .Subscribe();
            this.WhenAnyValue(x => x.HistoryItemSelectInd)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Where(i => i >= 0 && HistoryItems.Count > i)
                .Select(i => HistoryItems.ElementAt(i).HistoryItemId)
                .Do(guid => MessageBus.Current.SendMessage(guid))
                .Select(guid => _imageDataManager.GetImage(guid).ImageMat.ToWriteableBitmap())
                .ToPropertyEx(this, x => x.InputImg);

            //this.WhenAnyValue(x => x.HistoryItemSelectInd)
            //    .ObserveOn(RxApp.MainThreadScheduler)
            //    .Where(i => i >= 0 && HistoryItems.Count > i)
            //    .Select(i => _imageDataManager.GetImage(HistoryItems.ElementAt(i).HistoryItemId).ImageMat.ToWriteableBitmap())
            //    .ToPropertyEx(this, x => x.OutputImg);
        }

        private HistoryItem ConvertData(ImageData imageData)
        {
            var scaleY = 90d / imageData.ImageMat.Height;
            var dst = _rt.T(imageData.ImageMat.Resize(Size.Zero, scaleY, scaleY));
            var wtBitmap = dst.ToWriteableBitmap();

            return new HistoryItem { HistoryItemId = imageData.ImageId, HistoryItemTxtMark = imageData.TxtMarker, HistoryItemImg = wtBitmap };
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