using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MaterialDesignThemes.Wpf;

namespace Client.ViewModel
{
    public static class OpStaticMethod
    {
        public static (PackIconKind icon, string info) GetOpInfo<T>()
        {
            object[] attributes = typeof(T).GetCustomAttributes(true);
            OperationInfoAttribute attribute = attributes.FirstOrDefault() as OperationInfoAttribute;
            return (attribute.Icon, attribute.Info);
        }
    }
}