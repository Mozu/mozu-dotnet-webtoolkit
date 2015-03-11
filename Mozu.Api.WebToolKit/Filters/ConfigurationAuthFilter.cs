using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Mozu.Api.Logging;
using Mozu.Api.Resources.Platform;
using Mozu.Api.Security;

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
                throw new UnauthorizedAccessException();
            var request = filterContext.RequestContext.HttpContext.Request;
            var apiContext = new ApiContext(request.Headers);
            if (apiContext.TenantId == 0)
            {
                apiContext = new ApiContext(int.Parse(request.QueryString.Get("tenantId")));
            }

            try
            {
                var tenantResource = new TenantResource();
                var tenant = Task.Factory.StartNew(() => tenantResource.GetTenantAsync(apiContext.TenantId).Result, TaskCreationOptions.LongRunning).Result;
                
            }
            catch (ApiException exc)
            {
                _logger.Error(exc);
                throw new Exception(exc.Message);
            }
        }
    }
}
