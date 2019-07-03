using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImageGallery.Config;
using ImageGallery.Error;
using ImageGallery.Models.Gallery;
using ImageGallery.Services;
using ImageGalleryDb;
using ImageGalleryDb.Models;
using ImageGalleryDb.Models.ImageGallery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace ImageGallery.Controllers
{
    public class GuessWordController : Controller// BaseController
    {
        private NLog.Logger _logger;
        private readonly Data _data;
        private readonly ImageService _imageService;

        public GuessWordController(NLog.Logger logger, Data data, ImageService imageService)
        {
            _logger = logger;
            _data = data;
            this._imageService = imageService;
        }

        //[Route("")]
        //[HttpGet]
        //public IActionResult Index() {
        //    var url = _imageService.Random();
        //    return View((object)url);
        //}
    }
}
