using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
using OpenCVVision.Model.Common;
using OpenCVVision.Model.Interface.Tool;
using Stylet;

namespace OpenCVVision.Model.Tools.ImgPreProcess
{
    public enum ImgSpace
    {
        Gray,
        BGR,
        BGRA,
        HSV,

        YUV
    }

    internal class ImgCvtColorSpace : ToolBase
    {
        public ImgCvtColorSpace(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        public Mat Run(Mat src, ImgSpace imgSpace = ImgSpace.Gray, int chanel = 0)
        {
            var dst = new Mat();
            var remat = new Mat();

            Mat[] chanels;
            switch (imgSpace)
            {
                case ImgSpace.Gray:
                    Cv2.CvtColor(src, remat, ColorConversionCodes.BGR2GRAY);
                    break;

                case ImgSpace.BGR:
                    chanels = src.Split();
                    remat = chanels[chanel];
                    break;

                case ImgSpace.BGRA:
                    Cv2.CvtColor(src, dst, ColorConversionCodes.BGR2BGRA);
                    chanels = dst.Split();
                    remat = chanels[chanel];

                    break;

                case ImgSpace.HSV:
                    Cv2.CvtColor(src, dst, ColorConversionCodes.BGR2HSV);
                    chanels = dst.Split();
                    remat = chanels[chanel];
                    break;

                case ImgSpace.YUV:
                    Cv2.CvtColor(src, dst, ColorConversionCodes.BGR2YUV);
                    chanels = dst.Split();
                    remat = chanels[chanel];
                    break;

                default:
                    Cv2.CvtColor(src, remat, ColorConversionCodes.BGR2GRAY);
                    break;
            }

            return remat;
        }
    }
}