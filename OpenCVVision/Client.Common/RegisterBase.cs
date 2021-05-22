using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Splat;

namespace Client.Common
{
    public abstract class RegisterBase
    {
        protected IMutableDependencyResolver _mutable = Locator.CurrentMutable;

        public RegisterBase()
        {
            ConfigService();
        }

        public abstract void ConfigService();
    }
}