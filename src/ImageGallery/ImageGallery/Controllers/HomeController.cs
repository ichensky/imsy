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
    public class HomeController : Controller// BaseController
    {
        private NLog.Logger _logger;
        private readonly Data _data;
        private readonly ImageService _imageService;

        public HomeController(NLog.Logger logger, Data data, Services.ImageService imageService)
        {
            _logger = logger;
            _data = data;
            this._imageService = imageService;
        }

        [Route("")]
        [HttpGet]
        public async Task<IActionResult> Index() {
            var url = await _imageService.Random();
            return View((object)url);
        }

        [Route("error/404")]
        [HttpGet]
        public IActionResult Error404()
        {
            return View();
        }
        [Route("contact")]
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }
        [Route("policy")]
        [HttpGet]
        public IActionResult Policy()
        {
            return View();
        }
    }
}
