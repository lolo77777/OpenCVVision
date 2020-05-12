using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Stylet;

namespace OpenCVVision.Model.Interface
{
    internal interface IToolWindow
    {
        public void Run();

        public void Cancle();

        public void Restore();
    }
}