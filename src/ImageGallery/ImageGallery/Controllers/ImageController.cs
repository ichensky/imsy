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
    public class ImageController : Controller//: BaseController
    {
        private NLog.Logger _logger;
        private readonly Data _data;

        public ImageController(NLog.Logger logger, Data data)
        {
            _logger = logger;
            _data = data;
        }

        [Route("image/{strId}")]
        public async Task<IActionResult> Image(string strId)
        {
            if (string.IsNullOrEmpty(strId))
            {
                return null;
            }
            using var imContext = new ImContext(_data.Options);


            var image = await imContext.Image
                .Where(x=>x.SeoId==strId)
                .FirstOrDefaultAsync();
            if (image==null)
            {
                return null;
            }

            var strs = await imContext.ImageStr.Include(x=>x.Str).Where(x => x.ImageId == image.Id).ToListAsync();
            var tags = strs
                       .Where(y => y.IsTag == ImageGalleryDb.Models.Im.Boolean.True).Select(y => y.Str.Name).ToList();
            var topTags = strs.Where(x=>x.Score>0.9).Select(y => y.Str.Name).ToList();

            var descriptionCaptions = await imContext.DescriptionCaption
                .Where(x => x.ImageDescriptionCaptions.Any(y=>y.ImageId==image.Id)).ToListAsync();


            return View(new ImageResponce
            {
                Image = new Models.Gallery.Image
                {
                    Id = image.GoogleDriveId,
                    Title = image.Title,
                    DescriptionCaptions= descriptionCaptions
                       .Select(y => y.Name).ToList(),
                    Tags= tags,
                    TopTags= topTags,
                    DominantColors = strs
                       .Where(y => y.IsDominantColor == ImageGalleryDb.Models.Im.Boolean.True)
                       .Select(y => y.Str.Name).ToList(),
                    Width = image.GoogleDriveWidth,
                    Height = image.GoogleDriveHeight
                }
            });
        }
    }
}
