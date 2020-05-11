using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;

namespace OpenCVVision.Model.Event
{
    public class DisImgEvent
    {
        public DisImgEvent(Mat? src)
        {
            Src = src;
        }

        public Mat? Src { get; set; }
    }
}