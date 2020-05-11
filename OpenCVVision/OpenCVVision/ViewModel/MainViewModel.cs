using System;
using System.Collections.Generic;
using System.Text;
using Stylet;

namespace OpenCVVision.ViewModel
{
    public class MainViewModel : Screen
    {
        public MainViewModel(ImgViewModel imgViewModel, OperationViewModel operationViewModel)
        {
            ImgVM = imgViewModel;
            OperaVM = operationViewModel;
        }

        public ImgViewModel ImgVM { get; set; }
        public OperationViewModel OperaVM { get; set; }
    }
}