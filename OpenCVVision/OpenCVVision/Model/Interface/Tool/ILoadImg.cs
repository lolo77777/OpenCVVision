using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;

namespace OpenCVVision.Model.Interface.Tool
{
    internal interface ILoadImg : IDisposable
    {
        string ToolName { set; get; }

        Mat? Run();
    }
}