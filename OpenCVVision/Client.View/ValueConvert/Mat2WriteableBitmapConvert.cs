using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.View
{
    public class Mat2WriteableBitmapConvert : IBindingTypeConverter, IEnableLogger
    {
        public int GetAffinityForObjects(Type fromType, Type toType)
        {
            if (fromType == typeof(Mat))
            {
                return 100; // any number other than 0 signifies conversion is possible.
            }
            return 0;
        }

        public bool TryConvert(object from, Type toType, object conversionHint, out object result)
        {
            try
            {
                result = ((Mat)from).ToWriteableBitmap();
            }
            catch (Exception ex)
            {
                this.Log().Write("Couldn't convert object to type: " + toType, Splat.LogLevel.Error);
                result = null;
                return false;
            }

            return true;
        }
    }
}