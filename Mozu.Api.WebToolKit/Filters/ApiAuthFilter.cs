using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Mozu.Api.Resources.Platform;

namespace Mozu.Api.WebToolKit.Filters
{
    public class ApiAuthFilter : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            base.OnAuthorization(actionContext);
            var cookieToken = string.Empty;
            var formToken = string.Empty;
           

            IEnumerable<string> values;
            actionContext.Request.Headers.TryGetValues("cookieToken", out values);
            if (values != null) cookieToken = values.First();

            actionContext.Request.Headers.TryGetValues("formToken", out values);
            if (values != null) formToken = values.First();

            if (String.IsNullOrEmpty(formToken) || string.IsNullOrEmpty(cookieToken))
            {
                var content = actionContext.Request.Content.ReadAsFormDataAsync().Result;
                if (content != null)
                {
                    formToken = content["formToken"];
                    cookieToken = content["cookieToken"];
                }
            }

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

                var hash = Mozu.Api.Security.SHA256Generator.GetHash(string.Empty,apiContext.TenantId.ToString());
                if (apiContext.HMACSha256 != hash)
                    throw new UnauthorizedAccessException();

                //validate tenant access
                try
                {
                    var tenantResource = new TenantResource();
                    var tennat = Task.Factory.StartNew(() => tenantResource.GetTenantAsync(apiContext.TenantId).Result, TaskCreationOptions.LongRunning).Result;
                }
                catch (ApiException ae)
                {
                    throw new Exception(ae.Message);
                }
            }

            

        }
    }
}
