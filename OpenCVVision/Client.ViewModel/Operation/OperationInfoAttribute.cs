using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MaterialDesignThemes.Wpf;

namespace Client.ViewModel
{
    public class OperationInfoAttribute : Attribute
    {
        public PackIconKind Icon { get; private set; }
        public string Info { get; private set; }

        public OperationInfoAttribute(string info, PackIconKind icon)
        {
            Info = info;
            Icon = icon;
        }
    }
}