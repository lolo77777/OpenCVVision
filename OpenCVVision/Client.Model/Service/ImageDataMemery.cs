using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

using Client.Data;
using Client.Model.Entity;

using DynamicData;

using OpenCvSharp;

namespace Client.Model.Service
{
    public class ImageDataMemery : IImageDataManager
    {
        public Guid? CurrentId { get; set; }
        public Subject<Guid?> InputMatGuidSubject { get; set; } = new();
        public Mat OutputMat { get; set; }
        public Subject<Mat> OutputMatSubject { get; set; } = new();
        public SourceCache<ImageData, Guid> SourceCacheImageData { get; set; }

        public ImageDataMemery()
        {
            SourceCacheImageData = new SourceCache<ImageData, Guid>(t => t.ImageId);
            InputMatGuidSubject.Subscribe(guid => CurrentId = guid);
            OutputMatSubject.Subscribe(mat => OutputMat = mat.Clone());
            SampleData();
        }

        private void SampleData()
        {
            Mat mat = Cv2.ImRead(FilePath.Image.Ta);

            AddImage("Sam1", mat);
        }

        public bool AddImage(string imageMarkTxt, Mat mat)
        {
            var itemCount = SourceCacheImageData.Items.Where(it => it.TxtMarker.Equals(imageMarkTxt)).Count();
            if (itemCount.Equals(0))
            {
                try
                {
                    var guid = Guid.NewGuid();
                    SourceCacheImageData.AddOrUpdate(new ImageData { ImageId = guid, TxtMarker = imageMarkTxt, ImageMat = mat });
                    InputMatGuidSubject.OnNext(guid);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                var imageMarkTxtNew = $"{imageMarkTxt}_{DateTime.Now.ToShortTimeString()}";
                return AddImage(imageMarkTxtNew, mat);
            }
        }

        public void AddOutputImage(string outputImageMarkTxt)
        {
            var str = string.IsNullOrWhiteSpace(outputImageMarkTxt) ? "dst" : outputImageMarkTxt;
            AddImage(str, OutputMat);
        }

        public Mat GetCurrentMat()
        {
            if (CurrentId.HasValue)
            {
                return GetImage(CurrentId).ImageMat;
            }
            else
            {
                return null;
            }
        }

        public ImageData GetImage(Guid? guid)
        {
            if (guid.HasValue)
            {
                return SourceCacheImageData.Items.Single(t => t.ImageId.Equals(guid));
            }
            else
            {
                return null;
            }
        }

        public void RaiseCurrent()
        {
            InputMatGuidSubject.OnNext(CurrentId);
        }

        public bool RemoveCurrentImage()
        {
            if (CurrentId.HasValue)
            {
                var bol = RemoveImage(CurrentId);

                if (bol)
                {
                    if (SourceCacheImageData.Items.Count() > 0)
                    {
                        CurrentId = SourceCacheImageData.Items.LastOrDefault().ImageId;
                    }
                    else
                    {
                        CurrentId = null;
                    }
                    InputMatGuidSubject.OnNext(CurrentId);
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        public bool RemoveIamge(string imageMarkTxt)
        {
            var itemTmp = SourceCacheImageData.Items.Where(it => it.TxtMarker.Equals(imageMarkTxt)).Select(t => t).FirstOrDefault();
            try
            {
                if (itemTmp is not null)
                {
                    SourceCacheImageData.Remove(itemTmp);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool RemoveImage(Guid? guid)
        {
            try
            {
                SourceCacheImageData.RemoveKey(guid.Value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}