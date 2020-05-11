using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCVVision.Model.Common;
using OpenCVVision.Model.Interface.Tool;

namespace OpenCVVision.Model.Tools.LoadImg
{
    internal class LoadImgFromFile : ILoadImg
    {
        private OpenFileDialog openfiledialog = new OpenFileDialog();
        public string ToolName { get; set; } = "从文件加载";

        public void Dispose()
        {
            openfiledialog = null;
        }

        public Mat? Run()
        {
            openfiledialog.ShowDialog();
            if (!string.IsNullOrEmpty(openfiledialog.FileName))
            {
                var src = new Mat(openfiledialog.FileName);
                BaseRes.OriginSrc = src;
                return src;
            }
            else
            {
                return null;
            }
        }
    }
}