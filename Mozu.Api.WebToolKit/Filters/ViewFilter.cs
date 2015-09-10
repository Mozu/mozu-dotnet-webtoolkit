using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Mozu.Api.Logging;

namespace Mozu.Api.WebToolKit.Filters
{
     [AttributeUsage(AttributeTargets.Method)]
    public class ViewFilter : ActionFilterAttribute
    {
          private readonly ILogger _logger = LogManager.GetLogger(typeof(ConfigurationAuthFilter));

         public override void OnActionExecuting(ActionExecutingContext filterContext)
         {
             base.OnActionExecuting(filterContext);

             

             var authenticated = FilterUtils.Validate(filterContext.RequestContext.HttpContext.Request);
             if (authenticated) return;
             filterContext.HttpContext.Response.StatusCode = 401;
             filterContext.HttpContext.Response.End();
         }
    }
}
