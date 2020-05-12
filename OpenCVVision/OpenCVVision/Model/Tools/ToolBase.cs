using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
using OpenCVVision.Model.Common;
using OpenCVVision.Model.Event;
using Stylet;

namespace OpenCVVision.Model.Tools
{
    internal abstract class ToolBase
    {
        private IEventAggregator eventAggregator;

        public ToolBase(IEventAggregator _eventAggregator)
        {
            this.eventAggregator = _eventAggregator;
            InputMat = BaseRes.CurSrc;
        }

        public string Name { set; get; }
        public Mat InputMat { set; get; }
        public Mat OutputMat { set; get; }

        public void Cancle()
        {
            eventAggregator.Publish(new DisImgEvent(InputMat));
            BaseRes.CurSrc = InputMat.Clone();
        }

        public void Restore()
        {
            OutputMat = InputMat.Clone();
            eventAggregator.Publish(new DisImgEvent(InputMat));
            BaseRes.CurSrc = InputMat.Clone();
        }
    }
}