using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading.Tasks;
//using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Mozu.Api.Logging;
using Mozu.Api.Resources.Platform;
using Newtonsoft.Json;

namespace Mozu.Api.WebToolKit.Filters
{
    
    public class ApiAuthFilter : ActionFilterAttribute
    {
        private static readonly ILogger _logger = LogManager.GetLogger(typeof(ApiAuthFilter));

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var authenticated = FilterUtils.Validate(filterContext.HttpContext.Request);

            if (!authenticated)
            {
                filterContext.HttpContext.Response.StatusCode = (int)(HttpStatusCode.Unauthorized);
                filterContext.Result = new UnauthorizedResult();
            }
        }
    }
}
