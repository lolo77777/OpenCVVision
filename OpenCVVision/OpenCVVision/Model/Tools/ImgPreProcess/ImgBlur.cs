using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
using OpenCVVision.Model.Interface.Tool;
using Stylet;

namespace OpenCVVision.Model.Tools.ImgPreProcess
{
    internal enum BlurMethod
    {
        Mean, Median, Gauss
    }

    internal class ImgBlur : ToolBase
    {
        public ImgBlur(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }

        public Mat Run(Mat mat, BlurMethod blurMethod, int ksize = 3, int count = 1, float sigmaX = 0.4f, float sigmaY = 0f)
        {
            var dst = new Mat();
            var mattmp = mat.Clone();
            switch (blurMethod)
            {
                case BlurMethod.Mean:

                    for (int i = 0; i < count; i++)
                    {
                        mattmp = mattmp.Blur(new Size(ksize, ksize));
                    }
                    dst = mattmp.Clone();
                    break;

                case BlurMethod.Median:

                    for (int i = 0; i < count; i++)
                    {
                        mattmp = mattmp.MedianBlur(ksize);
                    }
                    dst = mattmp.Clone();
                    break;

                case BlurMethod.Gauss:
                    for (int i = 0; i < count; i++)
                    {
                        mattmp = mattmp.GaussianBlur(new Size(ksize, ksize), sigmaX, sigmaY);
                    }
                    dst = mattmp.Clone();
                    break;

                default:
                    break;
            }
            mattmp.Dispose();
            return dst;
        }
    }
}