using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageGallery.Logger
{
    public class HttpLogMiddleware
    {
        private readonly RequestDelegate _next;

        public HttpLogMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            // Do something with context near the beginning of request processing.
            var logger = LogManager.GetLogger(Logger.log.ToString());

            try
            {
                var request = context.Request;



                var body = string.Empty;
                request.EnableRewind();

                // Arguments: Stream, Encoding, detect encoding, buffer size 
                // AND, the most important: keep stream opened
                using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
                {
                    body = reader.ReadToEnd();
                }

                request.Body.Position = 0;

                var headers = HeadersToString(request.Headers);
                var msg = new StringBuilder($"HTTP Request:\r\n" +
                    $"==>IP:{context.Connection.RemoteIpAddress}\r\n" +
                    $"==>method:{request.Method}\r\n" +
                    $"==>path:{request.Path}\r\n");
                msg.AppendLine($"---->headers:\r\n{headers}");

                if (!string.IsNullOrEmpty(body))
                {
                    msg.AppendLine($"==>body:\r\n{body}");
                }
                logger.Trace(msg);
            }
            catch (Exception ex)
            {
                logger.Error($"HTTP Request: " + ex.ToString());
            }

            await _next.Invoke(context);

        }

        private StringBuilder HeadersToString(IHeaderDictionary headers)
        {
            var str = new StringBuilder();
            if (headers != null && headers.Values != null)
            {
                foreach (var item in headers)
                {
                    str.AppendLine($"{item.Key}:{item.Value}");
                }
            }
            return str;
        }


    }
}
