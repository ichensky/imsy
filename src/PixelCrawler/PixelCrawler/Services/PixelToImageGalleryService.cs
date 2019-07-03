using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EntityFrameworkCore.Sqlite.Migrations;
using ImageGalleryDb;
using ImageGalleryDb.Models;
using ImageGalleryDb.Models.Im;
using ImageGalleryDb.Models.Pixel;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;
using PixelCrawler.Helpers;

namespace PixelCrawler.Services
{
    public class PixelToImageGalleryService
    {
        private NLog.Logger _logger;
        private DbContextOptions _pixel_dbContextOptions;
        private DbContextOptions _im_dbContextOptions;

        public PixelToImageGalleryService(NLog.Logger logger,
            DbContextOptions pixel_dbContextOptions,
            DbContextOptions im_dbContextOptions
            )
        {
            _logger = logger;
            _pixel_dbContextOptions = pixel_dbContextOptions;
            _im_dbContextOptions = im_dbContextOptions;
        }
        //public async Task Process(int index = 0)
        //{

        //        using var pixelContext = new PixelContext(_pixel_dbContextOptions);
        //    var pixelImages = pixelContext.PixelImage
        //        .Where(x => x.GoogleId != null);
        //    int i = -1;

        //    using var dbContext = new ImageGalleryContext(_dbContextOptions);
        //    foreach (var pixelImage in pixelImages)
        //    {
        //        i++;
        //        _logger.Info($"Processing: {i}");
        //        var image = await dbContext.Image
        //            .SingleOrDefaultAsync(x => x.GoogleId == pixelImage.GoogleId);

        //        if (image == null)
        //        {
        //            var listKeywordIds = new List<Guid>();
        //            if (pixelImage.Keywords!=null)
        //            {
        //                var keywords = new Regex("[^a-z0-9 -,]").Replace(pixelImage.Keywords.ToLower(), "").Split(',')
        //                    .Distinct();
        //                foreach (var keyword in keywords)
        //                {
        //                    var kw = await dbContext.Keyword.SingleOrDefaultAsync(x => x.Value == keyword);

        //                    Guid keywordId;
        //                    if (kw == null)
        //                    {
        //                        keywordId = Guid.NewGuid();
        //                        await dbContext.Keyword.AddAsync(new Keyword
        //                        {
        //                            KeywordId = keywordId,
        //                            Value = keyword
        //                        });
        //                    }
        //                    else
        //                    {
        //                        keywordId = kw.KeywordId;
        //                    }

        //                    listKeywordIds.Add(keywordId);
        //                }

        //            }

        //            var im = new Image
        //            {
        //                ImageId = pixelImage.PixelImageId,
        //                GoogleId = pixelImage.GoogleId 
        //            };
        //            if (!string.IsNullOrWhiteSpace(pixelImage.Title))
        //            {
        //                im.Title = new Regex("[^a-zA-Z0-9 -,]").Replace(pixelImage.Title, "");
        //            }
        //            await dbContext.Image.AddAsync(im);
        //            foreach (var item in listKeywordIds)
        //            {
        //                await dbContext.ImageKeyword.AddAsync(new ImageKeyword
        //                {
        //                    ImageKeywordId=Guid.NewGuid(),
        //                    ImageId = pixelImage.PixelImageId,
        //                    KeywordId = item,
        //                });
        //            }
        //            await dbContext.SaveChangesAsync();
        //        }
        //    }

        //}



        private async Task<List<ImageGalleryDb.Models.ImageGallery.Image>> GenData()
        {
            using var pixelContext = new PixelContext(_pixel_dbContextOptions);
            var q = await pixelContext.PixelImage
                .Where(x => !string.IsNullOrWhiteSpace(x.GoogleId) &&
                (x.ProcessState == null || x.ProcessState.Value == 0)
                && !string.IsNullOrWhiteSpace(x.MSVision))
                //.Take(2000)
                .ToListAsync();

            var r = q.AsParallel().GroupBy(x =>
            {
                var obj = JsonConvert.DeserializeObject<ImageAnalysis>(x.MSVision);
                obj.RequestId = null;
                obj.Metadata = null;
                obj.Objects = null;
                var str = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
                { NullValueHandling = NullValueHandling.Ignore });
                var hash = Encoding.ASCII.GetBytes(str).GetStringSha1Hash();
                return hash;
            }).Select(x => x.FirstOrDefault())
            .Select(x =>
            {
                var xobj = JsonConvert.DeserializeObject<ImageAnalysis>(x.MSVision);
                var obj = new ImageGalleryDb.Models.ImageGallery.MSVision.MSVision();
                obj.Tags = xobj.Tags.Select(x => new ImageGalleryDb.Models.ImageGallery.MSVision.Tag
                {
                    Name = x.Name.ToLower(),
                    Confidence = (float)x.Confidence
                }).GroupBy(x=>x.Name).Select(x=>x.OrderByDescending(y=>y.Confidence).FirstOrDefault()).ToList();
                obj.Categories = xobj.Categories.Select(x => new ImageGalleryDb.Models.ImageGallery.MSVision.Category
                {
                    Name = x.Name.ToLower(),
                    Score = (float)x.Score
                }).GroupBy(x => x.Name).Select(x => x.OrderByDescending(y => y.Score).FirstOrDefault()).ToList();

                if (xobj.Description != null)
                {
                    obj.Description = new ImageGalleryDb.Models.ImageGallery.MSVision.Description
                    {
                        Tags = xobj.Description.Tags.Select(x => x.ToLower()).Distinct().ToList(),
                    };
                    if (xobj.Description.Captions != null)
                    {
                        obj.Description.Captions = xobj.Description.Captions.Select(
                            x => new ImageGalleryDb.Models.ImageGallery.MSVision.Caption
                            {
                                Confidence = (float)x.Confidence,
                                Text = x.Text?.ToLower().Replace("a close up of ", "")
                            }).GroupBy(x => x.Text).Select(x => x.OrderByDescending(y => y.Confidence).FirstOrDefault()).ToList();
                    }
                }
                if (xobj.Color != null)
                {
                    obj.Color = new ImageGalleryDb.Models.ImageGallery.MSVision.Color();
                    obj.Color.AccentColor = xobj.Color.AccentColor;
                    obj.Color.IsBWImg = xobj.Color.IsBWImg;
                    if (xobj.Color.DominantColors != null)
                    {
                        obj.Color.DominantColors = xobj.Color.DominantColors.Select(
                            x => new ImageGalleryDb.Models.ImageGallery.MSVision.DominantColor
                            {
                                Name = x.ToLower(),
                                DominantColorType = ImageGalleryDb.Models.ImageGallery.MSVision.DominantColorType.None
                            }).ToList();

                        var dcb = xobj.Color.DominantColorBackground.ToLower();
                        var dcf = xobj.Color.DominantColorForeground.ToLower();
                        obj.Color.DominantColors.ForEach(x =>
                        {
                            if (x.Name == dcb
                            && x.Name == dcf)
                            {
                                x.DominantColorType = ImageGalleryDb.Models.ImageGallery.MSVision.DominantColorType.BackgroundAndForeground;
                            }
                            else if (x.Name == dcb)
                            {
                                x.DominantColorType = ImageGalleryDb.Models.ImageGallery.MSVision.DominantColorType.Background;
                            }
                            else if (x.Name == dcf)
                            {
                                x.DominantColorType = ImageGalleryDb.Models.ImageGallery.MSVision.DominantColorType.Foreground;
                            }
                            else
                            {
                            }
                        });
                    }

                }



                var title = x.Title;
                if (string.IsNullOrEmpty(title))
                {
                    title = obj.Description.Captions.FirstOrDefault()?.Text;
                    if (string.IsNullOrEmpty(title))
                    {
                        title = obj.Tags.FirstOrDefault()?.Name;
                        if (string.IsNullOrEmpty(title))
                        {
                            title = obj.Description.Tags.FirstOrDefault();
                        }
                    }
                }
                var r = new ImageGalleryDb.Models.ImageGallery.Image
                {
                    MSVision = obj,
                    Title = title.ToLower(),
                    SeoId = title.Replace(' ', '-').ToLower(),
                    GoogleDrive = new ImageGalleryDb.Models.ImageGallery.GoogleDrive
                    {
                        Width = x.Width.Value,
                        Height = x.Height.Value,
                        GoogleId = x.GoogleId
                    }
                };

                return r;
            }).ToList();


            r.AsParallel().GroupBy(x => x.Title)
                .Select(x => new { x.Key, Count = x.Count(), Values = x.ToList() })
                .Where(x => x.Count > 1)
                .ToList().ForEach(x =>
                {
                    var urlTitle = x.Key;

                    for (int i = 1; i < x.Count; i++)
                    {
                        x.Values[i].SeoId = (urlTitle).Replace(' ', '-') + '-' + i;
                    }
                });
            return r;
        }

        public async Task<string> GenJsonDb()
        {

            var r = await GenData();
            _logger.Info("Data generated.");

            return JsonConvert.SerializeObject(r, new JsonSerializerSettings
            { NullValueHandling = NullValueHandling.Ignore });
        }


        private async Task<Dictionary<string, int>> GenDbCategories(List<ImageGalleryDb.Models.ImageGallery.Image> query)
        {
            var items = query
                .Where(x => x.MSVision.Categories != null)
               .SelectMany(x => x.MSVision.Categories.Select(y => y.Name))
               .Distinct()
               .Select(x => new ImageGalleryDb.Models.Im.Category
               {
                   Name = x
               })
               .ToList();
            await Insert(items);
            using var imContext = new ImContext(_im_dbContextOptions);
            return (await imContext.Category.ToListAsync())
                .ToDictionary(x => x.Name, x => x.Id);
        }

        private async Task<Dictionary<string, int>> GenDbDescriptionCaptions(List<ImageGalleryDb.Models.ImageGallery.Image> query)
        {
            var items = query
                .Where(x => x.MSVision.Description != null)
                .Where(x => x.MSVision.Description.Captions != null)
               .SelectMany(x => x.MSVision.Description.Captions.Select(y => y.Text))
               .Distinct()
               .Select(x => new DescriptionCaption
               {
                   Name = x
               })
               .ToList();
            await Insert(items);
            using var imContext = new ImContext(_im_dbContextOptions);
            return (await imContext.DescriptionCaption.ToListAsync())
                .ToDictionary(x => x.Name, x => x.Id);
        }


        private async Task<Dictionary<string, int>> GenDbStrs(List<ImageGalleryDb.Models.ImageGallery.Image> query)
        {
            var tags = query
                .Where(x => x.MSVision.Tags != null)
               .SelectMany(x => x.MSVision.Tags.Select(y => y.Name));
            var descriptionTags = query
                .Where(x => x.MSVision.Description != null)
                .Where(x => x.MSVision.Description.Tags != null)
               .SelectMany(x => x.MSVision.Description.Tags);
            var colors = query
                .Where(x => x.MSVision.Color != null)
                .Where(x => x.MSVision.Color.DominantColors != null)
               .SelectMany(x => x.MSVision.Color.DominantColors.Select(x => x.Name));
            var strings = new List<string>();
            strings.AddRange(tags);
            strings.AddRange(descriptionTags);
            strings.AddRange(colors);

            var strs = strings
                .Distinct()
                .Select(x => new Str { Name = x })
                .ToList();

            await Insert(strs);
            using var imContext = new ImContext(_im_dbContextOptions);
            return (await imContext.Str.ToListAsync())
                .ToDictionary(x => x.Name, x => x.Id);
        }


        private async Task<Dictionary<string, int>> GenDbImages(List<ImageGalleryDb.Models.ImageGallery.Image> query)
        {
            var items = query
                .Select(x => new Image
                {
                    GoogleDriveId = x.GoogleDrive.GoogleId,
                    GoogleDriveWidth = x.GoogleDrive.Width,
                    GoogleDriveHeight = x.GoogleDrive.Height,
                    SeoId = x.SeoId,
                    Title = x.Title,
                    MSVisionAccentColor = x.MSVision.Color?.AccentColor,
                    MSVisionIsBWImg = (x.MSVision.Color != null) && (x.MSVision.Color.IsBWImg)
                    ? ImageGalleryDb.Models.Im.Boolean.True
                    : ImageGalleryDb.Models.Im.Boolean.False
                }).ToList();
            await Insert(items);
            using var imContext = new ImContext(_im_dbContextOptions);
            return (await imContext.Image.ToListAsync())
                .ToDictionary(x => x.GoogleDriveId, x => x.Id);
        }

        private async Task GenDbImageCategories(
            List<ImageGalleryDb.Models.ImageGallery.Image> query,
            Dictionary<string, int> dictImages,
            Dictionary<string, int> dictCategories
            )
        {
            var items = query
                .Where(x => x.MSVision.Categories != null)
                .SelectMany(x => x.MSVision.Categories.Select(y =>
                new ImageCategory
                {
                    Score = y.Score,
                    CategoryId = dictCategories[y.Name],
                    ImageId = dictImages[x.GoogleDrive.GoogleId]
                }
                ))
                .ToList();
            await Insert(items);
        }
        private async Task GenDbImageDescriptionCaptions(
            List<ImageGalleryDb.Models.ImageGallery.Image> query,
            Dictionary<string, int> dictImages,
            Dictionary<string, int> dictDescriptionCaptions
            )
        {
            var items = query
                .Where(x => x.MSVision.Description != null)
                .Where(x => x.MSVision.Description.Captions != null)
                .SelectMany(x => x.MSVision.Description.Captions.Select(y =>
                new ImageDescriptionCaption
                {
                    Score = y.Confidence,
                    DescriptionCaptionId = dictDescriptionCaptions[y.Text],
                    ImageId = dictImages[x.GoogleDrive.GoogleId]
                }
                )).ToList();
            await Insert(items);
        }

        private async Task GenDbImageStrs_Tags(
            List<ImageGalleryDb.Models.ImageGallery.Image> query,
            Dictionary<string, int> dictImages,
            Dictionary<string, int> dictStrs
            )
        {
            var items = query
                .SelectMany(x =>
                {
                    var list = x.MSVision.Tags.Select(y =>
                new ImageStr
                {
                    Score = y.Confidence,
                    StrId = dictStrs[y.Name],
                    ImageId = dictImages[x.GoogleDrive.GoogleId],
                    IsTag = ImageGalleryDb.Models.Im.Boolean.True
                }).ToList();


                    if (x.MSVision.Description != null && x.MSVision.Description.Tags != null)
                    {
                        var list2 = x.MSVision.Description.Tags.Select(y =>
                            new ImageStr
                            {
                                StrId = dictStrs[y],
                                ImageId = dictImages[x.GoogleDrive.GoogleId],
                                IsDescriptionTag = ImageGalleryDb.Models.Im.Boolean.True
                            }).ToList();
                        foreach (var item in list2)
                        {
                            var el = list.SingleOrDefault(e => e.StrId == item.StrId);
                            if (el != null)
                            {
                                el.IsDescriptionTag = item.IsDescriptionTag;
                            }
                            else
                            {
                                list.Add(item);
                            }
                        }
                    }

                    if (x.MSVision.Color != null && x.MSVision.Color.DominantColors != null)
                    {
                        var list2 = x.MSVision.Color.DominantColors.Select(y =>
                        {
                            var tkl = new ImageStr
                            {
                                StrId = dictStrs[y.Name],
                                ImageId = dictImages[x.GoogleDrive.GoogleId],
                                IsDominantColor= ImageGalleryDb.Models.Im.Boolean.True
                            };
                            if (y.DominantColorType == ImageGalleryDb.Models.ImageGallery.MSVision.DominantColorType.BackgroundAndForeground)
                            {
                                tkl.IsDominantColorBackground = ImageGalleryDb.Models.Im.Boolean.True;
                                tkl.IsDominantColorForeground = ImageGalleryDb.Models.Im.Boolean.True;
                            }
                            else if (y.DominantColorType == ImageGalleryDb.Models.ImageGallery.MSVision.DominantColorType.Background) { 
                                tkl.IsDominantColorBackground = ImageGalleryDb.Models.Im.Boolean.True;
                            }
                            else if (y.DominantColorType == ImageGalleryDb.Models.ImageGallery.MSVision.DominantColorType.Foreground) { 
                                tkl.IsDominantColorForeground = ImageGalleryDb.Models.Im.Boolean.True;
                            }
                            return tkl;
                        }
                                ).ToList();
                        foreach (var item in list2)
                        {
                            var el = list.SingleOrDefault(e => e.StrId == item.StrId);
                            if (el != null)
                            {
                                el.IsDominantColor = item.IsDominantColor;
                                el.IsDominantColorBackground = item.IsDominantColorBackground;
                                el.IsDominantColorForeground = item.IsDominantColorForeground;
                            }
                            else
                            {
                                list.Add(item);
                            }
                        }
                    }


                    return list;
                }


                ).ToList();

            await Insert(items);
        }


        private async Task UpdateCategoryTopMin() {
            using var imContext = new ImContext(_im_dbContextOptions);

            var cats = await imContext.Category.ToListAsync();
            foreach (var item in cats)
            {
                var count = await imContext.ImageCategory.Where(x => x.CategoryId == item.Id).CountAsync();
                var take = 1000;
                var sc = imContext.ImageCategory.Where(x => x.CategoryId == item.Id)
                    .OrderByDescending(x => x.Score).Take(take)
                    .OrderBy(x => x.Score);
               

                var score = (await sc.FirstOrDefaultAsync()).Score;
                item.TopScoreMin = score;
            }

            await imContext.SaveChangesAsync();
        }
        private async Task UpdateTagTopMin()
        {
            using var imContext = new ImContext(_im_dbContextOptions);

            var strs = await imContext.Str.ToListAsync();
            foreach (var item in strs)
            {
                var count = await imContext.ImageStr.Where(x => x.StrId == item.Id).CountAsync();

                var take = 1000;
                var sc = imContext.ImageStr.Where(x => x.StrId == item.Id)
                    .Where(x => x.Score != null)
                    .OrderByDescending(x => x.Score).Take(take)
                    .OrderBy(x => x.Score);

                var score = (await sc.FirstOrDefaultAsync())?.Score;
                item.TopScoreMin = score;

                if (score!=null)
                {
                    var scMin = await imContext.ImageStr.Where(x => x.StrId == item.Id)
                    .Where(x => x.Score != null)
                    .OrderBy(x => x.Score).FirstOrDefaultAsync();

                    item.TopScoreMinMin = scMin.Score;
                }                
            }

            await imContext.SaveChangesAsync();
        }

        public async Task GenDb(string json)
        {
            var query = JsonConvert.DeserializeObject<List<ImageGalleryDb.Models.ImageGallery.Image>>(json);
            _logger.Info("Saving data to db...");

            var dictCategories = await GenDbCategories(query);
            var dictDescriptionCaptions = await GenDbDescriptionCaptions(query);
            var dictStrs = await GenDbStrs(query);
            var dictImages = await GenDbImages(query);
            await GenDbImageCategories(query, dictImages, dictCategories);
            await GenDbImageDescriptionCaptions(query, dictImages, dictDescriptionCaptions);

            await GenDbImageStrs_Tags(query, dictImages, dictStrs);

            _logger.Info($"Updating {nameof(UpdateCategoryTopMin)}...");
            await UpdateCategoryTopMin();
            _logger.Info($"Updating {nameof(UpdateTagTopMin)}...");
            await UpdateTagTopMin();


            _logger.Info("Saving data to db...done");
        }

        private async Task Insert<T>(List<T> list) where T : class
        {
            var val = 50000;
            for (int i = 0; i < list.Count; i += val)
            {
                _logger.Info($"saving {typeof(T).Name}: {i}/{list.Count}");
                using var imContext = new ImContext(_im_dbContextOptions);
                imContext.ChangeTracker.AutoDetectChangesEnabled = false;
                await imContext.AddRangeAsync(list.Skip(i).Take((val)).ToArray());
                await imContext.SaveChangesAsync();
            }
        }
    }
}
