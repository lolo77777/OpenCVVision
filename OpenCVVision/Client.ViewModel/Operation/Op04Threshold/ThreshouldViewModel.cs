using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Common;

using ReactiveUI.Fody.Helpers;

namespace Client.ViewModel.Operation
{
    [OperationInfo("二值化")]
    public class ThreshouldViewModel : OperaViewModelBase
    {
        [Reactive] public BarViewModel BarViewModelSam => Register.ResolveVM<BarViewModel>();
    }
}