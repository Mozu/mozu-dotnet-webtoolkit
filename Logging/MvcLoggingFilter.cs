using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Mvc.Filters;
//using System.Web;
//using System.Web.Mvc;
using Microsoft.AspNetCore.Http.Extensions;
using Mozu.Api.Logging;
using Microsoft.AspNetCore.Http;

namespace Mozu.Api.WebToolKit.Logging
{
    public class MvcLoggingFilter : IActionFilter//ActionFilterAttribute
    {
        
        private readonly ILogger _logger = LogManager.GetLogger(typeof(MvcLoggingFilter));

       
        public  void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var req = filterContext.HttpContext.Request;


            var corrId =  Guid.NewGuid();
            req.Headers.Add(Headers.X_VOL_CORRELATION, corrId.ToString());

            Trace.CorrelationManager.ActivityId = corrId;

            var requestInfo = string.Format("{0} {1}", req.Method, req.GetDisplayUrl());

            _logger.Debug(string.Format("{0}, Originating IP - {1} ", requestInfo, req.Host.Value));

            if (_logger.IsDebugEnabled)
            {
                _logger.Debug(String.Format("Start Time: {0}",DateTime.Now));
                _logger.Debug( RequestBody(req));

                var headers = string.Empty;
                foreach (var key in req.Headers.Keys)
                {
                    if (!String.IsNullOrEmpty(headers)) headers += ", ";
                    headers += string.Format("{0} : {1}", key, req.Headers[key]);
                }
                _logger.Debug(headers);
            }

            //base.OnActionExecuting(filterContext);

            var res = filterContext.HttpContext.Response;
            res.Headers.Add(Headers.X_VOL_CORRELATION, corrId.ToString());

            if (_logger.IsDebugEnabled)
            {
                _logger.Debug(String.Format("End Time: {0}", DateTime.Now));
            }

        }


        private static string RequestBody(HttpRequest req)
        {
            var bodyStream = new StreamReader(req.Body);
            bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
            var bodyText = bodyStream.ReadToEnd();
            return bodyText;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            throw new NotImplementedException();
        }
    }
}
