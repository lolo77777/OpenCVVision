using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
using OpenCVVision.Model.Interface;

namespace OpenCVVision.Model.Data
{
    internal class OperaHistory : IOperaHistory
    {
        public List<(string Name, Mat OutputMat)> Record { set; get; } = new List<(string Name, Mat OutputMat)>();

        public OperaHistory()
        {
        }
    }
}