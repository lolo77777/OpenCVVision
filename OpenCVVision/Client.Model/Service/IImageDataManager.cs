using Client.Model.Entity;

using DynamicData;

using OpenCvSharp;

using System;
using System.Reactive.Subjects;

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
        ImageData GetImage(string markTxt);
        bool IsExsitByMark(string markTxt);
        public void RaiseCurrent();
        bool RemoveCurrentImage();
        bool RemoveIamge(string imageMarkTxt);
        bool RemoveImage(Guid? guid);
    }
}