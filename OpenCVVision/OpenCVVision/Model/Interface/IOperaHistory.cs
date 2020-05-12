using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;

namespace OpenCVVision.Model.Interface
{
    public interface IOperaHistory
    {
        List<(string Name, Mat OutputMat)> Record { set; get; }
    }
}