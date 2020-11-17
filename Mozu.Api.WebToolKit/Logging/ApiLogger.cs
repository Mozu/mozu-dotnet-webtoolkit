using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
//using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.IO;
using Mozu.Api.Logging;

namespace Mozu.Api.WebToolKit.Logging
{
    //public class ApiLogger : DelegatingHandler
    //{
    //    //private readonly bool _isRequestResponseLoggingEnabled = false;
    //    private readonly ILogger _logger = LogManager.GetLogger(typeof (ApiLogger));



    //    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    //    {
    //        Guid corrId = Guid.NewGuid();
    //        if (request.Headers.Contains(Headers.X_VOL_CORRELATION))
    //        {
    //            var headerValues = request.Headers.GetValues(Headers.X_VOL_CORRELATION);
    //            if (headerValues != null && !Equals(headerValues, Enumerable.Empty<string>()))
    //                Guid.TryParse(headerValues.FirstOrDefault(), out corrId);
    //        } else
    //            request.Headers.Add(Headers.X_VOL_CORRELATION, corrId.ToString());

    //        Trace.CorrelationManager.ActivityId = corrId;

    //        var requestInfo = string.Format("{0} {1}", request.Method, request.RequestUri);

    //        _logger.Info(requestInfo);

    //        if (_logger.IsDebugEnabled)
    //        {
    //           _logger.Debug(String.Format("Start Time: {0}", DateTime.Now));
    //           _logger.Debug( request.Content.ReadAsStringAsync().Result);

    //           var headers = request.Headers.ToList().Select(x => string.Format("{0} : {1}", x.Key, x.Value.FirstOrDefault())).Aggregate((x,y)=>x+" , "+y);
    //           _logger.Debug(String.Format("{0} Start Time: {1}", corrId, headers));
    //        }

    //        var response = await base.SendAsync(request, cancellationToken);

    //        response.Headers.Add(Headers.X_VOL_CORRELATION, corrId.ToString());

    //        try
    //        {
    //            if (_logger.IsDebugEnabled)
    //            {
    //                if (response.IsSuccessStatusCode && response.Content != null)
    //                    _logger.Debug(response.Content.ReadAsStringAsync().Result);

    //                _logger.Debug(String.Format("End Time: {0}", DateTime.Now));
    //            }

    //        }
    //        catch (Exception exc){}


    //        return response;
    //    }
    //}

    public class ApiLogger
    {
        //private readonly bool _isRequestResponseLoggingEnabled = false;
        private readonly RequestDelegate _next;
        private readonly ILogger _logger = LogManager.GetLogger(typeof(ApiLogger));
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        public ApiLogger(RequestDelegate next)
        {
            _next = next;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await LogRequest(httpContext);
            await LogResponse(httpContext);
        }

        private async Task LogRequest(HttpContext context)
        {
            var request = context.Request;
            Guid corrId = Guid.NewGuid();
            if(request.Headers.ContainsKey(Headers.X_VOL_CORRELATION))
            {
                var headerValues = request.Headers[Headers.X_VOL_CORRELATION].ToArray();
                if (headerValues != null && !Equals(headerValues, Enumerable.Empty<string>()))
                    Guid.TryParse(headerValues.FirstOrDefault(), out corrId);
            }
            else
            {
                request.Headers.Add(Headers.X_VOL_CORRELATION, corrId.ToString());
            }
            Trace.CorrelationManager.ActivityId = corrId;

            var requestInfo = string.Format("{0} {1}", request.Method,new Uri(request.GetDisplayUrl()));

            _logger.Info(requestInfo);

            if (_logger.IsInfoEnabled)
            {
                //Adding required headers till the APIAuthFilter cookies are fixed

                //if (!request.Headers.ContainsKey(Headers.X_VOL_TENANT))
                //{
                //    request.Headers.Add(Headers.X_VOL_TENANT, "18239");
                //}

                //if (!request.Headers.ContainsKey(Headers.USERID))
                //{
                //    request.Headers.Add(Headers.USERID, "355060a60a5e48eeb7f2fb8d92af2ba5");
                //}


                HttpRequestRewindExtensions.EnableBuffering(request);// request.EnableBuffering();
                var requestStream = _recyclableMemoryStreamManager.GetStream();
                await context.Request.Body.CopyToAsync(requestStream);
                _logger.Info(String.Format("Start Time: {0}", DateTime.UtcNow));
                _logger.Info(ReadStreamInChunks(requestStream));
                request.Body.Position = 0;
                var headers = request.Headers.ToList().Select(x => string.Format("{0} : {1}", x.Key, x.Value.FirstOrDefault())).Aggregate((x, y) => x + " , " + y);
                _logger.Info(String.Format("{0} Start Time: {1}", corrId, headers));
                await requestStream.DisposeAsync();
            }

        }

        private async Task LogResponse(HttpContext context)
        {
            var response = context.Response;
            var corrId = Trace.CorrelationManager.ActivityId;
            response.Headers.Add(Headers.X_VOL_CORRELATION, corrId.ToString());

            try
            {
                if (_logger.IsInfoEnabled)
                {
                    
                    if (response.StatusCode==(int)HttpStatusCode.OK  && response.Body !=null)
                    {
                        var originalBodyStream = response.Body;
                        var responseBody = _recyclableMemoryStreamManager.GetStream();
                        response.Body = responseBody;
                        await _next(context);
                        response.Body.Seek(0, SeekOrigin.Begin);
                        var text = await new StreamReader(response.Body).ReadToEndAsync();
                        response.Body.Seek(0, SeekOrigin.Begin);
                        _logger.Info(text);
                        await responseBody.CopyToAsync(originalBodyStream);
                        await responseBody.DisposeAsync();
                    }
                    _logger.Info(String.Format("End Time: {0}", DateTime.UtcNow));
                }

            }
            catch (Exception exc) 
            {
                _logger.Info(exc.Message,exc);
            }
        }

        private static string ReadStreamInChunks(Stream stream)
        {
            const int readChunkBufferLength = 4096;

            stream.Seek(0, SeekOrigin.Begin);

            var textWriter = new StringWriter();
            var reader = new StreamReader(stream);

            var readChunk = new char[readChunkBufferLength];
            int readChunkLength;

            do
            {
                readChunkLength = reader.ReadBlock(readChunk,
                                                   0,
                                                   readChunkBufferLength);
                textWriter.Write(readChunk, 0, readChunkLength);
            } while (readChunkLength > 0);

            return textWriter.ToString();
        }

    }

    public static class ApiLoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiLogger>();
        }
    }
}
