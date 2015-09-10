using System;
using System.Net;
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
                apiContext = new ApiContext(request.Form);

            if (apiContext.TenantId == 0) //if not found load from query string
            {
                var tenantId = request.QueryString.Get("tenantId");
                if (String.IsNullOrEmpty(tenantId))
                {
                    filterContext.HttpContext.Response.StatusCode = 401;
                    filterContext.HttpContext.Response.End();
                }
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
                filterContext.HttpContext.Response.StatusCode = 401;
                filterContext.HttpContext.Response.End();
            }
            
            string cookieToken;
            string formToken;
            filterContext.HttpContext.Request.Cookies.Remove("userId");

            AntiForgery.GetTokens(null, out cookieToken, out formToken);
            filterContext.HttpContext.Response.Cookies.Add(GetCookie("formToken", formToken));
            filterContext.HttpContext.Response.Cookies.Add(GetCookie("cookieToken", cookieToken));
            filterContext.HttpContext.Response.Cookies.Add(GetCookie("tenantId", apiContext.TenantId.ToString()));
            if (!string.IsNullOrEmpty(apiContext.UserId))
                filterContext.HttpContext.Response.Cookies.Add(GetCookie("userId", apiContext.UserId));

            var hashString = string.Concat(apiContext.TenantId.ToString(), cookieToken, formToken);
            if (!string.IsNullOrEmpty(apiContext.UserId))
            {
                _logger.Info("Adding userid to hash :" + apiContext.UserId);
                hashString = string.Concat(hashString, apiContext.UserId);
            }
            var hash = SHA256Generator.GetHash(string.Empty, hashString);
            _logger.Info("Computed Hash : " + hash);
            filterContext.HttpContext.Response.Cookies.Add(GetCookie("hash", HttpUtility.UrlEncode(hash)));
        }

        public HttpCookie GetCookie(string name, string value)
        {
            var cookie = new HttpCookie(name, value) {Expires = DateTime.UtcNow.AddHours(1)};

            return cookie;
        }
    }
}
