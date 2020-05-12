using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
using OpenCVVision.Model.Interface.Tool;
using Stylet;

namespace OpenCVVision.Model.Tools.ImgPreProcess
{
    internal class ImgThreshold : ToolBase
    {
        public ImgThreshold(IEventAggregator _eventAggregator) : base(_eventAggregator)
        {
            Name = "常用二值化";
        }

        public Mat Run(Mat inMat,int thresh,int setvalue,ThresholdTypes thresholdmethod)
        {
            var dst = InputMat.Clone();
            var tmpmat = dst.EmptyClone();
            switch (thresholdmethod)
            {
                case ThresholdTypes.Binary:
                    tmpmat = dst.Threshold(thresh,setvalue,ThresholdTypes.Binary);
                    break;

                case ThresholdTypes.BinaryInv:
                    tmpmat = dst.Threshold(thresh,setvalue,ThresholdTypes.BinaryInv);
                    break;

                case ThresholdTypes.Trunc:
                    tmpmat = dst.Threshold(thresh,setvalue,ThresholdTypes.Trunc);
                    break;

                case ThresholdTypes.Tozero:
                    tmpmat = dst.Threshold(thresh,setvalue,ThresholdTypes.Tozero);
                    break;

                case ThresholdTypes.TozeroInv:
                    tmpmat = dst.Threshold(thresh,setvalue,ThresholdTypes.TozeroInv);
                    break;

                case ThresholdTypes.Mask:
                    tmpmat = dst.Threshold(thresh,setvalue,ThresholdTypes.Mask);
                    break;

                case ThresholdTypes.Otsu:
                    tmpmat = dst.Threshold(thresh,setvalue,ThresholdTypes.Otsu);
                    break;

                case ThresholdTypes.Triangle:
                    tmpmat = dst.Threshold(thresh,setvalue,ThresholdTypes.Triangle);
                    break;

                default:
                    break;
            }

            return tmpmat;
        }
    }
}