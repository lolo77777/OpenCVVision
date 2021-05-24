using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Model.Entity;

using DynamicData;

using OpenCvSharp;

namespace Client.Model.Service
{
    public interface IImageDataManager
    {
        SourceCache<ImageData, Guid> SourceCacheImageData { set; get; }

        bool AddImage(string imageMarkTxt, Mat mat);

        ImageData GetImage(Guid guid);

        bool RemoveIamge(string imageMarkTxt);

        bool RemoveImage(Guid guid);
    }
}