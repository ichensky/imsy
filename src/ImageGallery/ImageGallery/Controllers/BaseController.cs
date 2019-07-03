using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ImageGallery.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ImageGallery.Controllers
{
    [Route("api/[controller]/[action]")]
    [Produces("application/json")]
    [Exception(Logger.Logger.log)]
    //[ResponseCache(Duration = 60 * 60 * 24, VaryByQueryKeys = new[] { "*" })]
    public class BaseController : Controller
    {

    }
}
