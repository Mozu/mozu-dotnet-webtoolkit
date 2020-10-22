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
    public class ApiAuthFilter : AuthorizeAttribute, IAuthorizationFilter
    {
        private static readonly ILogger _logger = LogManager.GetLogger(typeof (ApiAuthFilter));

        public void OnAuthorization(AuthorizationFilterContext actionContext)
        {
            //base.OnAuthorization(actionContext);


            var authenticated = FilterUtils.Validate(actionContext.HttpContext.Request);

            if (!authenticated)
            {
                actionContext.HttpContext.Response.StatusCode = (int)(HttpStatusCode.Unauthorized);
                actionContext.Result= new UnauthorizedResult();
                return;
            }
        }
    }
}
