using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Model.Entity;

using DynamicData;

using OpenCvSharp;

namespace Client.Model.Service
{
    public class ImageDataMemery : IImageDataManager
    {
        public SourceCache<ImageData, Guid> SourceCacheImageData { get; set; }

        public ImageDataMemery()
        {
            SourceCacheImageData = new SourceCache<ImageData, Guid>(t => t.ImageId);
            SampleData();
        }

        private void SampleData()
        {
            Mat mat = Cv2.ImRead(@"E:\Pictures\高清壁纸Z\ta(1)(1).bmp");

            AddImage("Sam1", mat);
            var mat1 = Cv2.ImRead(@"E:\Pictures\高清壁纸Z\kaer.jpg");
            AddImage("Sam1", mat1);
        }

        public bool AddImage(string imageMarkTxt, Mat mat)
        {
            var itemCount = SourceCacheImageData.Items.Where(it => it.TxtMarker.Equals(imageMarkTxt)).Count();
            if (itemCount.Equals(0))
            {
                try
                {
                    SourceCacheImageData.AddOrUpdate(new ImageData { ImageId = Guid.NewGuid(), TxtMarker = imageMarkTxt, ImageMat = mat });
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                var imageMarkTxtNew = string.Concat(imageMarkTxt, DateTime.Now.ToShortTimeString());
                return AddImage(imageMarkTxtNew, mat);
            }
        }

        public ImageData GetImage(Guid guid)
        {
            return SourceCacheImageData.Items.Single(t => t.ImageId.Equals(guid));
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

        public bool RemoveImage(Guid guid)
        {
            try
            {
                SourceCacheImageData.RemoveKey(guid);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}