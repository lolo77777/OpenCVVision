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
        public static (double id, PackIconKind icon, string info) GetOpInfo<T>()
        {
            object[] attributes = typeof(T).GetCustomAttributes(true);
            OperationInfoAttribute attribute = attributes.FirstOrDefault() as OperationInfoAttribute;
            return (attribute.Id, attribute.Icon, attribute.Info);
        }

        public static (double id, PackIconKind icon, string info) GetOpInfo(Type type)
        {
            object[] attributes = type.GetCustomAttributes(true);
            OperationInfoAttribute attribute = attributes.FirstOrDefault() as OperationInfoAttribute;
            return (attribute.Id, attribute.Icon, attribute.Info);
        }
    }
}