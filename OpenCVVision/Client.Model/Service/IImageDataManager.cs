using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

using Client.Model.Entity;

using DynamicData;

using OpenCvSharp;

namespace Client.Model.Service
{
    public interface IImageDataManager
    {
        Guid? CurrentId { set; get; }
        Subject<Guid?> InputMatGuidSubject { set; get; }
        Mat OutputMat { get; set; }
        Subject<Mat> OutputMatSubject { set; get; }
        SourceCache<ImageData, Guid> SourceCacheImageData { set; get; }

        bool AddImage(string imageMarkTxt, Mat mat);

        void AddOutputImage(string outputImageMarkTxt);

        Mat GetCurrentMat();

        ImageData GetImage(Guid? guid);

        public void RaiseCurrent();

        bool RemoveCurrentImage();

        bool RemoveIamge(string imageMarkTxt);

        bool RemoveImage(Guid? guid);
    }
}