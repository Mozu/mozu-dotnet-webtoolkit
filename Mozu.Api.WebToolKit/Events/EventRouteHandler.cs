//using System.Web;
//using System.Web.Routing;
//using Autofac;
using Mozu.Api.Events;
using System;

namespace Mozu.Api.WebToolKit.Events
{
    public class EventRouteHandler 
    {
        public IServiceProvider Container;

        public EventRouteHandler(IServiceProvider iComponentContext)
        {
            Container = iComponentContext;
        }
    }
}
