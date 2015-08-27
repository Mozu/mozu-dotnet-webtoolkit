using System;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Mozu.Api.Logging;
using Mozu.Api.Resources.Platform;
using Mozu.Api.Security;
using Newtonsoft.Json;

namespace Mozu.Api.WebToolKit.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ConfigurationAuthFilter : ActionFilterAttribute
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(ConfigurationAuthFilter));

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (!ConfigurationAuth.IsRequestValid(filterContext.HttpContext.Request))
                throw new SecurityException("Unauthorized");

            var request = filterContext.RequestContext.HttpContext.Request;
            var apiContext = new ApiContext(request.Headers); //try to load from headers
            if (apiContext.TenantId == 0) //try to load from body
                apiContext = new ApiContext(request.Params);

            if (apiContext.TenantId == 0) //if not found load from query string
            {
                var tenantId = request.QueryString.Get("tenantId");
                if (String.IsNullOrEmpty(tenantId))
                    throw new UnauthorizedAccessException();
                apiContext = new ApiContext(int.Parse(tenantId));
            }

            try
            {
                var tenantResource = new TenantResource();
                var tenant = Task.Factory.StartNew(() => tenantResource.GetTenantAsync(apiContext.TenantId).Result, TaskCreationOptions.LongRunning).Result;
            }
            catch (ApiException exc)
            {
                _logger.Error(exc);
                throw new SecurityException("Unauthorized");
            }
            
            string cookieToken;
            string formToken;

            AntiForgery.GetTokens(null, out cookieToken, out formToken);
            filterContext.HttpContext.Response.Cookies.Add(new HttpCookie("formToken", formToken));
            filterContext.HttpContext.Response.Cookies.Add(new HttpCookie("cookieToken", cookieToken));
            filterContext.HttpContext.Response.Cookies.Add(new HttpCookie("tenantId", apiContext.TenantId.ToString()));
            var hashString = SHA256Generator.GetHash(string.Empty, string.Concat(apiContext.TenantId.ToString(), cookieToken, formToken));

            filterContext.HttpContext.Response.Cookies.Add(new HttpCookie("hash", HttpUtility.UrlEncode(hashString)));
        }
    }
}
