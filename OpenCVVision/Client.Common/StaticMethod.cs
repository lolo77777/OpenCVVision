using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Common
{
    public static class StaticMethod
    {
        /// <summary>
        /// 获取类型的特性信息，用来做操作面板VM标记
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetInfo<T>()
        {
            object[] attributes = typeof(T).GetCustomAttributes(true);
            OperationInfoAttribute attribute = attributes.FirstOrDefault() as OperationInfoAttribute;
            return attribute.Info;
        }
    }
}