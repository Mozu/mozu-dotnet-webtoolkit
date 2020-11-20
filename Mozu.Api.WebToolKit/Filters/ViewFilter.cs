using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
//using System.Web.Mvc;
using Mozu.Api.Logging;

namespace Mozu.Api.WebToolKit.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ViewFilter : ActionFilterAttribute
    {
          private readonly ILogger _logger = LogManager.GetLogger(typeof(ViewFilter));

         public override void OnActionExecuting(ActionExecutingContext filterContext)
         {
             base.OnActionExecuting(filterContext);
             var authenticated = FilterUtils.Validate(filterContext.HttpContext.Request);
             if (authenticated) return;
             filterContext.HttpContext.Response.StatusCode = 401;
            filterContext.Result = new UnauthorizedResult();
         }
    }
}
