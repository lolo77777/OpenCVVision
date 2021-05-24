using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Common
{
    public static class StaticMethod
    {
        public static string GetInfo(Type t)
        {
            object[] attributes = t.GetCustomAttributes(true);
            OperationInfoAttribute attribute = attributes[0] as OperationInfoAttribute;
            return attribute.Info;
        }
    }
}