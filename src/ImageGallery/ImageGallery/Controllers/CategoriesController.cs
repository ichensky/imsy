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
using ImageGalleryDb;
using ImageGalleryDb.Models;
using ImageGalleryDb.Models.ImageGallery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace ImageGallery.Controllers
{
    public class CategoriesController : Controller//: BaseController
    {
        private NLog.Logger _logger;
        private readonly Data _data;

        public CategoriesController(NLog.Logger logger, Data data)
        {
            _logger = logger;
            _data = data;
        }

        [Route("categories")]
        public IActionResult Index()
        {
            return View(_data.SortedCategories);
        }

    }
}
