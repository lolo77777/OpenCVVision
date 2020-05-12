using System;
using System.Collections.Generic;
using System.Text;
using Stylet;

namespace OpenCVVision.ViewModel
{
    public class MainViewModel : Screen
    {
        public MainViewModel(ImgViewModel imgViewModel,OperationViewModel operationViewModel,OperationHistoryViewModel operationHistoryViewModel)
        {
            ImgVM = imgViewModel;
            OperaVM = operationViewModel;
            OperaHistoryVM = operationHistoryViewModel;
        }

        public ImgViewModel ImgVM { get; set; }
        public OperationViewModel OperaVM { get; set; }
        public OperationHistoryViewModel OperaHistoryVM { get; set; }
    }
}