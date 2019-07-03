using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImageGallery.Config;
using ImageGallery.Error;
using ImageGallery.Models.Gallery;
using ImageGallery.Models.Tags;
using ImageGallery.Services;
using ImageGalleryDb;
using ImageGalleryDb.Models.Im;
using ImageGalleryDb.Models.ImageGallery;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using SomeApplicationNamespace;

namespace ImageGallery.Controllers
{
    public class QueryImage
    {
        public ImageGalleryDb.Models.ImageGallery.MSVision.Category Category { get; set; }
        public ImageGalleryDb.Models.ImageGallery.Image Image { get; set; }
        public ImageGalleryDb.Models.ImageGallery.MSVision.Tag Tag { get; set; }
    }
    public class GalleryController : Controller//: BaseController
    {
        private const int PageCount = 10;
        private readonly Random _random = new Random();
        private NLog.Logger _logger;
        private readonly Data _data;
        private readonly ImageService _imageService;

        public GalleryController(NLog.Logger logger, Data data, Services.ImageService imageService)
        {
            this._logger = logger;
            this._data = data;
            this._imageService = imageService;
        }


        [Route("api/Gallery/Logo")]
        [Produces("application/json")]
        [ResponseCache(Duration = 1)]
        [HttpGet]
        public async Task<IActionResult> Logo(int h = 220)
        {
            var url = await _imageService.Random(h);
            return Redirect(url);
        }

        [Produces("application/json")]
        [Route("api/Gallery/Images")]
        [HttpPost]
        public async Task<ErrorResponce> Images([FromBody]ImageQuery imageQuery)
        {
            if (imageQuery == null ||
                string.IsNullOrWhiteSpace(imageQuery.Query)
                || imageQuery.Query.Length > 128)
            {
                return new ErrorResponce { ErrorCode = ErrorCode.BadRequestParams };
            }
            var query = imageQuery.Query;
            var page = imageQuery.Page;

            _logger.Info($"query: {query}; page: {page}");
            query = query.ToLower();
            query = Regex.Replace(query, @"[^a-zA-Z_ ]+", " ");
            query = Regex.Replace(query, @"[ ]+", " ");
            query = Regex.Replace(query, @"[__]+", "_");
            query = query.Trim();

            var arr = query.Split(' ').Distinct().ToList();
            arr.Remove("and");
            arr.Remove("or");

            if (arr.Count > 6 || arr.Count == 0)
            {
                return new ErrorResponce { ErrorCode = ErrorCode.BadRequestParams };
            }

            var possibleCategories = arr.Where(x => x.Contains('_')).ToList();
            possibleCategories.ForEach(x => arr.Remove(x));
            if (possibleCategories.Count > 1
                || (possibleCategories.Count == 0 && arr.Count == 0))
            {
                return new ImagesResponce();
            }

            using var imContext = new ImContext(_data.Options);

            //IQueryable<ImageGalleryDb.Models.Im.Image> imQ = null;

            var isBW = ImageGalleryDb.Models.Im.Boolean.False;
            if (arr.Count > 0)
            {
                var colors = _data.Colors.Where(x => arr.Contains(x)).ToList();
                if (colors.Count == 2
                      && colors.Contains("black")
                      && colors.Contains("white"))
                {
                    isBW = ImageGalleryDb.Models.Im.Boolean.True;
                }
            }

            var imas = imContext.Image.AsQueryable();

            if (isBW== ImageGalleryDb.Models.Im.Boolean.True)
            {
                imas = imContext.Image.Where(x => x.MSVisionIsBWImg == ImageGalleryDb.Models.Im.Boolean.True);
            }
            if (possibleCategories.Count > 0)
            {
                var category = (from im in imContext.Category
                             where im.Name == possibleCategories[0]
                             select im).FirstOrDefault();
                if (category == null)
                {
                    return new ImagesResponce();
                }
                       
                imas = from im in imas
                      join imc in imContext.ImageCategory on im.Id equals imc.ImageId
                      where imc.CategoryId==category.Id&& imc.Score > (category.TopScoreMin-0.001)
                      orderby imc.Score descending
                      select im;
            }
            if (arr.Count == 1)
            {
                var tag = (from im in imContext.Str
                                where im.Name == arr[0]
                                select im).FirstOrDefault();
                if (tag == null)
                {
                    return new ImagesResponce();
                }
                if (tag.TopScoreMin != null)
                {
                    imas = from im in imas
                           join imc in imContext.ImageStr on im.Id equals imc.ImageId
                           where imc.StrId==tag.Id && imc.Score > (tag.TopScoreMin-0.001)
                           orderby imc.Score descending
                           select im;
                }
                else
                {
                    imas = (from im in imas
                           join imc in imContext.ImageStr on im.Id equals imc.ImageId
                           where imc.StrId == tag.Id 
                           select im).Take(1000);
                }
            }
            else if (arr.Count > 1) {
                var word = string.Join(' ', arr);
                var tagWord = (from im in imContext.Str
                           where im.Name == word
                               select im).FirstOrDefault();
                if (tagWord != null)
                {
                    if (tagWord.TopScoreMin != null)
                    {
                        imas = from im in imas
                               join imc in imContext.ImageStr on im.Id equals imc.ImageId
                               where imc.StrId == tagWord.Id && imc.Score > (tagWord.TopScoreMin-0.001)
                               orderby imc.Score descending
                               select im;
                    }
                    else
                    {
                        imas = (from im in imas
                                join imc in imContext.ImageStr on im.Id equals imc.ImageId
                                where imc.StrId == tagWord.Id
                                select im).Take(1000);
                    }
                }
                else {
                    var imags = await (from im in imContext.Str
                                 where arr.Contains(im.Name)
                                 orderby im.TopScoreMin descending
                                       select im).ToListAsync();
                    if (imags.Count!=arr.Count)
                    {
                        return new ImagesResponce();
                    }
                    if (imags[0].TopScoreMin == null)
                    {
                        var ids = imags.Select(x => x.Id).ToList();
                        imas = imContext.ImageStr.Where(x => ids.Contains(x.StrId))
                                .Take(5000)
                                .GroupBy(x => x.Image)
                                .Where(x => x.Count() == arr.Count)
                                .Select(x => x.Key);
                    }
                    else
                    {
                            var ids = imags.Skip(1).Select(x => x.Id).ToList();
                        
                            if (imags[1].TopScoreMinMin != null)
                            {
                                var ids2 = ids.Skip(1).ToList();

                            imas = imContext.ImageStr.Where(x =>
                                (x.StrId == imags[0].Id && x.Score > (imags[0].TopScoreMin-0.001))
                                || (x.StrId == imags[1].Id && x.Score > (imags[1].TopScoreMinMin-0.001))
                                || ids2.Contains(x.StrId))
                          .GroupBy(x => x.Image)
                          .Where(x => x.Count() == arr.Count)
                          .OrderByDescending(x => x.Sum(y => y.Score))
                          .Select(x => x.Key);

                        }
                            else
                            {

                            imas = imContext.ImageStr.Where(x =>
                                (x.StrId == imags[0].Id && x.Score > (imags[0].TopScoreMin-0.001))
                                || ids.Contains(x.StrId))
                              .GroupBy(x => x.Image)
                              .Where(x => x.Count() == arr.Count)
                              .OrderByDescending(x => x.Sum(y => y.Score))
                              .Select(x => x.Key);
                        }

                    }

                   
                }
            }
           
            _logger.Info($"Preparing: query: {query}; page: {page}");
            var result = new List<Models.Gallery.Image>();

            //var sql = imas.ToSql();


     //       Benchmark(() => {
     //          var list= imas
     //.Skip(page * PageCount).Take(PageCount)
     //.Select(x => new Models.Gallery.Image
     //{
     //    Url = $"https://drive.google.com/thumbnail?id={x.GoogleDriveId}&sz=w480",
     //    StrId = x.SeoId,
     //})
     //.ToList();
     //       }, 10);

            result = await imas
      .Skip(page * PageCount).Take(PageCount)
      .Select(x => new Models.Gallery.Image
      {
          Url = $"https://drive.google.com/thumbnail?id={x.GoogleDriveId}&sz=w480",
          StrId = x.SeoId,
      })
      .ToListAsync();
            

            _logger.Info($"Done: query: {query}; page: {page}");

            return new ImagesResponce
            {
                Images = result
            };
        }
        //private void Benchmark(Action act, int iterations)
        //{
        //    for (int i = 0; i < iterations; i++)
        //    {
        //        GC.Collect();
        //        Stopwatch sw = Stopwatch.StartNew();
        //        act.Invoke();
        //        sw.Stop();
        //        Console.WriteLine((sw.ElapsedMilliseconds / iterations).ToString());
        //    }
        //}


        [Route("gallery/{query}")]
        [HttpGet]
        public IActionResult Index(string query)
        {

            query = query.ToLower();
            query = Regex.Replace(query, @"[^a-zA-Z_ ]+", " ");
            query = Regex.Replace(query, @"[ ]+", " ");
            query = Regex.Replace(query, @"[__]+", "_");
            query = query.Trim();
            return View((object)query);
        }
    }
}
