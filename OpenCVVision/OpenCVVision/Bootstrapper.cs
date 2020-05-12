using System;
using Stylet;
using StyletIoC;

using System.IO;
using OpenCVVision.ViewModel;
using OpenCVVision.Model.Interface;
using OpenCVVision.Model.Data;

namespace OpenCVVision
{
    public class Bootstrapper : Bootstrapper<MainViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Configure the IoC container in here

            base.ConfigureIoC(builder);
            //builder.Bind<ICam>().To<Basler_pia2400>().InSingletonScope();
            builder.Bind<IOperaHistory>().To<OperaHistory>().InSingletonScope();
        }

        protected override void Configure()
        {
            // Perform any other configuration before the application starts
        }
    }
}