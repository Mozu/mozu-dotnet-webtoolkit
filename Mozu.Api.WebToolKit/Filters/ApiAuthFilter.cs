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
            var cookieToken = string.Empty;
            var formToken = string.Empty;

            var tenantId = GetCookie(actionContext, "tenantId");
            if (String.IsNullOrEmpty(tenantId)) throw new UnauthorizedAccessException();

            var hash = GetCookie(actionContext, "hash");
            if (string.IsNullOrEmpty(hash)) throw new UnauthorizedAccessException();

            actionContext.Request.Headers.Add(Headers.X_VOL_TENANT, tenantId);
            actionContext.Request.Headers.Add(Headers.X_VOL_HMAC_SHA256, hash);

            formToken = GetCookie(actionContext, "formToken");
            if (string.IsNullOrEmpty(formToken)) throw new UnauthorizedAccessException();

            cookieToken = GetCookie(actionContext, "cookieToken");
            if (string.IsNullOrEmpty(cookieToken)) throw new UnauthorizedAccessException();
            
            try
            {
                AntiForgery.Validate(cookieToken, formToken);
            }
            catch (Exception)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            //Validate tenant access
            var apiContext = new ApiContext(actionContext.Request.Headers);
            if (apiContext.TenantId >= 0)
            {
                if (String.IsNullOrEmpty(apiContext.HMACSha256))
                    throw new UnauthorizedAccessException();

                var computedHash = Security.SHA256Generator.GetHash(string.Empty, String.Concat(apiContext.TenantId.ToString(), cookieToken, formToken));
                if (apiContext.HMACSha256 != computedHash)
                {
                    _logger.Info("Header hash : " + apiContext.HMACSha256);
                    _logger.Info("Computed hash : " + computedHash);
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                try
                {
                    var tenantResource = new TenantResource();
                    var tennat = Task.Factory.StartNew(() => tenantResource.GetTenantAsync(apiContext.TenantId).Result, TaskCreationOptions.LongRunning).Result;
                }
                catch (ApiException ae)
                {
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }
        }

        private string GetCookie(HttpActionContext actionContext,string name)
        {
            var cookies = actionContext.Request.Headers.GetCookies(name).FirstOrDefault();
            if (cookies == null) return null;
            var cookie = cookies.Cookies.SingleOrDefault(x => x.Name == name);
            if (cookie == null) return null;
            return cookie.Value;
        }
    }
}
