using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Common
{
    public class OperationInfoAttribute : Attribute
    {
        public string Info { get; private set; }

        public OperationInfoAttribute(string info)
        {
            this.Info = info;
        }
    }
}