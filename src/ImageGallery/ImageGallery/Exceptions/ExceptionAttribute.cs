using ImageGallery.Error;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ImageGallery.Exceptions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method,
      AllowMultiple = false, Inherited = true)]
    public class ExceptionAttribute : ExceptionFilterAttribute
    {
        private NLog.Logger _logger;

        public ExceptionAttribute(Logger.Logger logger)
        {
            _logger = LogManager.GetLogger(logger.ToString());
        }
        public override void OnException(ExceptionContext context)
        {
            var r = new ErrorResponce { ErrorCode = ErrorCode.ServerError };
            var code = HttpStatusCode.InternalServerError;

            _logger.Error(context.Exception);

            if (context.Exception is InvalidDataException)
            {
                r.ErrorCode = ErrorCode.BadRequestParams;
                code = HttpStatusCode.BadRequest;
            }
            else if (context.Exception is Microsoft.Data.Sqlite.SqliteException) {
                r.ErrorCode = ErrorCode.ServerError;
                code = HttpStatusCode.InternalServerError;
            }

            context.HttpContext.Response.StatusCode = (int)code;
            context.Result = new JsonResult(r);

            base.OnException(context);
        }


    }
}
