//using System.Web;
//using System.Web.Routing;
//using Autofac;
using Mozu.Api.Events;
using System;

namespace Mozu.Api.WebToolKit.Events
{
    public class EventRouteHandler //: IRouteHandler
    {
        public IServiceProvider Container;

        public EventRouteHandler(IServiceProvider iComponentContext)
        {
            Container = iComponentContext;
        }

        //public IHttpHandler GetHttpHandler(RequestContext requestContext)
        //{
        //    return Container.GetService(EventHttpHandler);
        //}
    }
}
