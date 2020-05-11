using System;
using Stylet;
using StyletIoC;

using System.IO;
using OpenCVVision.ViewModel;

namespace OpenCVVision
{
    public class Bootstrapper : Bootstrapper<MainViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Configure the IoC container in here

            base.ConfigureIoC(builder);
            //builder.Bind<ICam>().To<Basler_pia2400>().InSingletonScope();
        }

        protected override void Configure()
        {
            // Perform any other configuration before the application starts
        }
    }
}