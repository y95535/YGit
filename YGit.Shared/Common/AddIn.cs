using System;
using System.Collections.Generic;
using System.Text;
using Autofac;

namespace YGit.Common
{
    internal class AddIn
    {
        public void RegsisterTypes()
        {
            GlobaService.ContainerBuilder.RegisterType<Logger>().As<ILogger>().PropertiesAutowired();
            GlobaService.Container = GlobaService.ContainerBuilder.Build();
        }
    }
}
