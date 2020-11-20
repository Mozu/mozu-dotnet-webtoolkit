using System;
using System.Collections.Generic;
using System.Reflection;
//using System.Web.Http;
//using System.Web.Mvc;
//using Autofac;
//using Autofac.Integration.Mvc;
//using Autofac.Integration.WebApi;
using Mozu.Api.Contracts.InstalledApplications;
using Mozu.Api.Events;
using Mozu.Api.Logging;
using Mozu.Api.ToolKit;
using Mozu.Api.WebToolKit.Events;
using Mozu.Api.WebToolKit.Logging;
using Mozu.Api.WebToolKit.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace Mozu.Api.WebToolKit
{
    public abstract class AbstractWebApiBootstrapper : AbstractBootstrapper
    {
        public  void InitializeContainer(IServiceCollection containerBuilder)
        {
            base.Configure(containerBuilder);
            containerBuilder.AddTransient<EventRouteHandler>();
            containerBuilder.AddTransient<MvcLoggingFilter>();
            containerBuilder.AddTransient<VersionController>();
            containerBuilder.AddTransient<ApplicationController>();
        }

    }
}
