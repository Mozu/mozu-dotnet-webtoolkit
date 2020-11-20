using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
//using System.Web;
//using System.Web.Helpers;
using Mozu.Api.Logging;
using Mozu.Api.Resources.Platform;
using Mozu.Api.Resources.Platform.Adminuser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Mozu.Api.Extensions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.DependencyInjection;

namespace Mozu.Api.WebToolKit.Filters
{
    internal class FilterUtils
    {
        private static readonly ILogger _logger = LogManager.GetLogger(typeof(FilterUtils));


        public static bool Validate(HttpRequest request)
        {
            try
            {
                var tenantId = GetCookie(request.Cookies, "tenantId");
                if (string.IsNullOrEmpty(tenantId)) throw new UnauthorizedAccessException();

                var hash = GetCookie(request.Cookies, "hash");
                if (string.IsNullOrEmpty(hash)) throw new UnauthorizedAccessException();
                var returnUrl = GetCookie(request.Cookies, Headers.X_VOL_RETURN_URL);

                var userId = GetCookie(request.Cookies, "userId");
                request.Headers.Add(Headers.X_VOL_TENANT, tenantId);
                request.Headers.Add(Headers.X_VOL_HMAC_SHA256, WebUtility.UrlDecode(hash));
                if (!string.IsNullOrEmpty(returnUrl))
                    request.Headers.Add(Headers.X_VOL_RETURN_URL, WebUtility.UrlDecode(returnUrl));

                if (!request.GetDisplayUrl().Contains(tenantId)) return false;

                if (!string.IsNullOrEmpty(userId))
                    request.Headers.Add(Headers.USERID, userId);
                var apiContext = new ApiContext(request.Headers.DictionaryToNVCollection());

                var formToken = GetCookie(request.Cookies, "formToken");
                if (string.IsNullOrEmpty(formToken)) return false;

                var cookieToken = GetCookie(request.Cookies, "cookieToken");
                if (string.IsNullOrEmpty(cookieToken)) return false;

                var isSubNavLink = GetCookie(request.Cookies, "subNavLink") == "1";
                return !string.IsNullOrEmpty(cookieToken) && Validate(request, apiContext, WebUtility.UrlDecode(formToken), WebUtility.UrlDecode(cookieToken), isSubNavLink);
            }
            catch (Exception exc)
            {

                _logger.Error(exc.Message, exc);
            }
            return false;
        }

        

        private static bool Validate(HttpRequest request,IApiContext apiContext, string formToken, string cookieToken, bool isSubNavLink)
        {

            AntiforgeryTokenSet tokens = null;
            try
            {
                var antiForgery = request.HttpContext.RequestServices.GetService<IAntiforgery>();
                tokens = antiForgery.GetTokens(request.HttpContext);
                
            }
            catch (Exception exc)
            {
                _logger.Error(exc.Message, exc);
                return false;
            }

            //Validate tenant access

            if (apiContext.TenantId <= 0) return false;
            if (String.IsNullOrEmpty(apiContext.HMACSha256))
                throw new UnauthorizedAccessException();

            var stringToHash = String.Concat(apiContext.TenantId.ToString(), cookieToken, formToken);
            if (!String.IsNullOrEmpty(apiContext.UserId) && isSubNavLink)
            {
                _logger.Info("Userid:" + apiContext.UserId);
                stringToHash = String.Concat(stringToHash, apiContext.UserId);
            }
            var computedHash = Security.SHA256Generator.GetHash(string.Empty, stringToHash );
            if (apiContext.HMACSha256 != computedHash)
            {
                _logger.Info("Header hash : " + WebUtility.UrlDecode(apiContext.HMACSha256));
                _logger.Info("Computed hash : " + computedHash);
                return false;
            }

            try
            {
                var tenantResource = new TenantResource();
                var tennat = Task.Factory.StartNew(() => tenantResource.GetTenantAsync(apiContext.TenantId).Result, TaskCreationOptions.LongRunning).Result;

            }
            catch (ApiException ae)
            {
                return false;
            }

            return true;
        }

        public static string GetCookie(IRequestCookieCollection cookieCollection, string name)
        {
            var cookie = cookieCollection[name];
            return  (cookie == null ? null :  cookie);
        }
    }
}
