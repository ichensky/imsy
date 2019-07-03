using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.Error
{
    public enum ErrorCode
    {
        Success = 0,
        BadRequestParams = 1,
        AccessDenied = 2,
        ServerError = 3,
    }
}
