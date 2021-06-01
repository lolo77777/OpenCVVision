using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Client.Common;

namespace Client.ViewModel.Operation
{
    [OperationInfo("轮廓")]
    public class ContoursViewModel : OperaViewModelBase
    {
        public ContoursViewModel()
        {
        }

        private void Updateoutput()
        {
            SendTime(() =>
            {
            });
        }
    }
}