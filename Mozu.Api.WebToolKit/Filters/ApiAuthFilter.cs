using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
//using System.Runtime.Remoting.Contexts;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
//using System.Web.Helpers;
//using System.Web.Http.Controllers;
//using System.Web.Http.Filters;
using Mozu.Api.Logging;
using Mozu.Api.Resources.Platform;
using Newtonsoft.Json;

namespace Mozu.Api.WebToolKit.Filters
{
    //public class ApiAuthFilter : AuthorizeAttribute,IAuthorizationFilter
    //{
    //    private static readonly ILogger _logger = LogManager.GetLogger(typeof (ApiAuthFilter));

    //    public void OnAuthorization(AuthorizationFilterContext actionContext)
    //    {
    //        //base.OnAuthorization(actionContext);


    //        var authenticated = FilterUtils.Validate(actionContext.HttpContext.Request);

    //        if (!authenticated)
    //        {
    //            actionContext.HttpContext.Response.StatusCode = (int)(HttpStatusCode.Unauthorized);
    //            actionContext.Result= new UnauthorizedResult();
    //        }
    //    }
    //}

    public class ApiAuthFilter : ActionFilterAttribute//AuthorizeAttribute, IAuthorizationFilter
    {
        private static readonly ILogger _logger = LogManager.GetLogger(typeof(ApiAuthFilter));

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            //var request = actionContext.HttpContext.Request;
            //var keyvalue = new KeyValuePair<string, string>("Sample", "OnLocalHost");
            //request.Cookies.Append(keyvalue);
            //Need to uncomment this code for debugging purpose.
            //var request = actionContext.HttpContext.Request;
            //if (!request.Headers.ContainsKey(Headers.X_VOL_TENANT))
            //{
            //    request.Headers.Add(Headers.X_VOL_TENANT, "18239");
            //}

            //if (!request.Headers.ContainsKey(Headers.USERID))
            //{
            //    request.Headers.Add(Headers.USERID, "355060a60a5e48eeb7f2fb8d92af2ba5");
            //}
            //return;

            var authenticated = FilterUtils.Validate(actionContext.HttpContext.Request);

            if (!authenticated)
            {
                actionContext.HttpContext.Response.StatusCode = (int)(HttpStatusCode.Unauthorized);
                actionContext.Result = new UnauthorizedResult();
            }
        }
    }
}
