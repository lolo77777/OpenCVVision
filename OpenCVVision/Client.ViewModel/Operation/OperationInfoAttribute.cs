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
        public double Id { get; private set; }
        public string Info { get; private set; }

        public OperationInfoAttribute(double id, string info, PackIconKind icon)
        {
            Info = info;
            Icon = icon;
            Id = id;
        }
    }
}