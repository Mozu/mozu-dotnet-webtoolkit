using System;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Mozu.Api.Extensions;
using Microsoft.AspNetCore.Antiforgery;
//using System.Web;
//using System.Web.Helpers;

using Mozu.Api.Logging;
using Mozu.Api.Resources.Platform;
using Mozu.Api.Security;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Mozu.Api.WebToolKit.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ConfigurationAuthFilter : ActionFilterAttribute
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(ConfigurationAuthFilter));
        //private readonly IAntiforgery _antiforgery;
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (!ConfigurationAuth.IsRequestValid(filterContext.HttpContext.Request))
            {
                filterContext.HttpContext.Response.StatusCode = 401;
                filterContext.Result = new UnauthorizedResult();
                return;
            }

            var request = filterContext.HttpContext.Request;
            var apiContext = new ApiContext(request.Headers.DictionaryToNVCollection()); //try to load from headers
            if (apiContext.TenantId == 0)
            {
                apiContext = new ApiContext(request.Form.FormCollectionToNVCollection());
            }
            if (apiContext.TenantId == 0) //if not found load from query string
            {
                var tenantId = request.Query["tenantId"].ToString();
                if (String.IsNullOrEmpty(tenantId))
                {
                    filterContext.HttpContext.Response.StatusCode = 401;
                    filterContext.Result = new UnauthorizedResult();
                    return;
                }
                //apiContext = new ApiContext(int.Parse(tenantId));
                apiContext.TenantId = int.Parse(tenantId);
            }
            var requestUri = filterContext.HttpContext.Request.PathBase.Value.Split('/');
            string path ="/"+ requestUri[1] + "/" + apiContext.TenantId.ToString();
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddHours(1),
                Path = path,
                Secure = true,
                HttpOnly = true,
                SameSite=SameSiteMode.None,
                IsEssential=true
                //Domain= new Uri(request.GetEncodedUrl()).Host
            };
            filterContext.HttpContext.Response.Cookies.Append("subNavLink", (String.IsNullOrEmpty(apiContext.UserId) ? "0" : "1"), cookieOptions);

            try
            {
                var tenantResource = new TenantResource();
                var tenant = Task.Factory.StartNew(() => tenantResource.GetTenantAsync(apiContext.TenantId).Result, TaskCreationOptions.LongRunning).Result;
            }
            catch (ApiException exc)
            {
                _logger.Error(exc.Message,exc);
                filterContext.HttpContext.Response.StatusCode = 401;
                filterContext.Result = new UnauthorizedResult();
                return;
            }
            
            var antiforgery = filterContext.HttpContext.RequestServices.GetService<IAntiforgery>();
            var tokens = antiforgery.GetTokens(filterContext.HttpContext);//.GetTokens(null, out cookieToken, out formToken);
            string cookieToken=tokens.CookieToken;
            string formToken=tokens.FormFieldName;
            
            filterContext.HttpContext.Response.Cookies.Append("formToken", WebUtility.UrlEncode(formToken),cookieOptions);
            filterContext.HttpContext.Response.Cookies.Append("cookieToken", WebUtility.UrlEncode(cookieToken), cookieOptions);
            filterContext.HttpContext.Response.Cookies.Append("tenantId", apiContext.TenantId.ToString(), cookieOptions);
            filterContext.HttpContext.Response.Cookies.Append(Headers.X_VOL_RETURN_URL, WebUtility.UrlEncode(apiContext.ReturnUrl), cookieOptions);
            if (!string.IsNullOrEmpty(apiContext.UserId))
                filterContext.HttpContext.Response.Cookies.Append(Headers.USERID, apiContext.UserId, cookieOptions);
            else
                filterContext.HttpContext.Response.Cookies.Delete(Headers.USERID);
            var hashString = string.Concat(apiContext.TenantId.ToString(), cookieToken, formToken);
            if (!string.IsNullOrEmpty(apiContext.UserId))
            {
                _logger.Info("Adding userid to hash :" + apiContext.UserId);
                hashString = string.Concat(hashString, apiContext.UserId);
            }
            var hash = SHA256Generator.GetHash(string.Empty, hashString);
            _logger.Info("Computed Hash : " + hash);
            filterContext.HttpContext.Response.Cookies.Append("hash", WebUtility.UrlEncode(hash), cookieOptions);
            
        }

       
    }
}
