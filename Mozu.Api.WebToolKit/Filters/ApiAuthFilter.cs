using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Mozu.Api.Logging;
using Mozu.Api.Resources.Platform;
using Newtonsoft.Json;

namespace Mozu.Api.WebToolKit.Filters
{
    public class ApiAuthFilter : AuthorizationFilterAttribute
    {
        private static readonly ILogger _logger = LogManager.GetLogger(typeof (ApiAuthFilter));

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            base.OnAuthorization(actionContext);


            var authenticated = FilterUtils.Validate(actionContext.Request);

            if (!authenticated)
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            
        }
    }
}
