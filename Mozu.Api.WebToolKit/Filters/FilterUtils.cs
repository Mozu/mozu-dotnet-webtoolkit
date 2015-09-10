using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using Mozu.Api.Logging;
using Mozu.Api.Resources.Platform;
using Mozu.Api.Resources.Platform.Adminuser;

namespace Mozu.Api.WebToolKit.Filters
{
    internal class FilterUtils
    {
        private static readonly ILogger _logger = LogManager.GetLogger(typeof(FilterUtils));


        public static bool Validate(HttpRequestBase request)
        {
            var tenantId = GetCookie(request.Cookies, "tenantId");
            if (string.IsNullOrEmpty(tenantId)) throw new UnauthorizedAccessException();

            var hash = GetCookie(request.Cookies, "hash");
            if (string.IsNullOrEmpty(hash)) throw new UnauthorizedAccessException();

            var userId = GetCookie(request.Cookies, "userId");
            request.Headers.Add(Headers.X_VOL_TENANT, tenantId);
            request.Headers.Add(Headers.X_VOL_HMAC_SHA256, hash);

            if (!string.IsNullOrEmpty(userId))
                request.Headers.Add(Headers.USERID, userId);
            var apiContext = new ApiContext(request.Headers);

            var formToken = GetCookie(request.Cookies, "formToken");
            if (string.IsNullOrEmpty(formToken)) return false;

            var cookieToken = GetCookie(request.Cookies, "cookieToken");
            return !string.IsNullOrEmpty(cookieToken) && Validate(apiContext, formToken, cookieToken);
        }

        public static bool Validate(HttpRequestMessage request)
        {
            var tenantId = GetCookie(request.Headers, "tenantId");
            if (String.IsNullOrEmpty(tenantId)) throw new UnauthorizedAccessException();

            var hash =  GetCookie(request.Headers, "hash");
            if (string.IsNullOrEmpty(hash)) throw new UnauthorizedAccessException();

            var userId = GetCookie(request.Headers, Headers.USERID);


            request.Headers.Add(Headers.X_VOL_TENANT, tenantId);
            request.Headers.Add(Headers.X_VOL_HMAC_SHA256, hash);
            if (!string.IsNullOrEmpty(userId))
                request.Headers.Add(Headers.USERID, userId);

            var apiContext = new ApiContext(request.Headers);

            var formToken = GetCookie(request.Headers, "formToken");
            if (string.IsNullOrEmpty(formToken)) return false;

            var cookieToken = GetCookie(request.Headers, "cookieToken");
            return !string.IsNullOrEmpty(cookieToken) && Validate(apiContext,formToken, cookieToken);
        }

        private static bool Validate(IApiContext apiContext, string formToken, string cookieToken)
        {
            

            try
            {
                AntiForgery.Validate(cookieToken, formToken);
            }
            catch (Exception)
            {
                return false;
            }

            //Validate tenant access

            if (apiContext.TenantId < 0) return false;
            if (String.IsNullOrEmpty(apiContext.HMACSha256))
                throw new UnauthorizedAccessException();

            var stringToHash = String.Concat(apiContext.TenantId.ToString(), cookieToken, formToken);
            if (!String.IsNullOrEmpty(apiContext.UserId))
            {
                _logger.Info("Userid:" + apiContext.UserId);
                stringToHash = String.Concat(stringToHash, apiContext.UserId);
            }
            var computedHash = Security.SHA256Generator.GetHash(string.Empty, stringToHash );
            if (HttpUtility.UrlDecode(apiContext.HMACSha256) != computedHash)
            {
                _logger.Info("Header hash : " + HttpUtility.UrlDecode(apiContext.HMACSha256));
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

        public static string GetCookie(HttpRequestHeaders headers, string name)
        {
            var cookies = headers.GetCookies(name).FirstOrDefault();
            if (cookies == null) return null;
            var cookie = cookies.Cookies.SingleOrDefault(x => x.Name == name);
            return cookie == null ? null : cookie.Value;
        }

        public static string GetCookie(HttpCookieCollection cookieCollection, string name)
        {
            var cookie = cookieCollection[name];
            return  (cookie == null ? null :  cookie.Value);
        }
    }
}
