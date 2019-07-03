using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageGalleryDb;
using ImageGalleryDb.Models.Pixel;
using ImageMagick;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;
using PixelCrawler.Helpers;
using PixelCrawler.Services;

namespace PixelCrawler.Parsers
{
    public class PixelService
    {
        private NLog.Logger _logger;
        private DbContextOptions _dbContextOptions;
        private UsersParser _usersParser;
        private ImagesParser _imagesParser;
        private ImageMetaParser _imageMetaParser;
        private FileLoader _fileLoader;
        private GoogleService _googleService;
        private MSVisionService _msvisionService;

        public PixelService(NLog.Logger logger, DbContextOptions dbContextOptions, 
            UsersParser usersParser, ImagesParser imagesParser, ImageMetaParser imageMetaParser,
            FileLoader fileLoader, GoogleService googleService, MSVisionService msvisionService) {
            this._logger = logger;
            this._dbContextOptions = dbContextOptions;
            this._usersParser = usersParser;
            this._imagesParser = imagesParser;
            this._imageMetaParser = imageMetaParser;
            this._fileLoader = fileLoader;
            this._googleService = googleService;
            this._msvisionService = msvisionService;
        }
        public async Task UpdateUsers(int page = 1) 
        {
            while (true)
            {
                _logger.Info($"Page: {page}");
                var list = await _usersParser.LoadUsersByPage(page);
                if (list is null || list.Count == 0)
                {
                    return;
                }
                _logger.Info($"Users loaded: {list.Count}");
                using (var pixelContext = new PixelContext(_dbContextOptions))
                {
                    foreach (var item in list)
                    {
                        var entity = await pixelContext.PixelUser.SingleOrDefaultAsync(x=>x.NickName == item.nickName);
                        if (entity == null)
                        {
                            await pixelContext.PixelUser.AddAsync(new PixelUser()
                            {
                                NickName = item.nickName,
                                PixelUserId = Guid.NewGuid(),
                                UserName = item.userName
                            });
                        }
                        else
                        {
                            entity.UserName = entity.UserName;
                            pixelContext.PixelUser.Update(entity);
                        }
                    }

                    await pixelContext.SaveChangesAsync();
                }

                page++;
            }
        }

        public async Task UpdateUsersImages(int index =0)
        {
            PixelUser user = null;
            do
            {
                _logger.Info($"index: {index}");
                using (var pixelContext = new PixelContext(_dbContextOptions))
                {
                    user = await pixelContext.PixelUser.Skip(index).FirstOrDefaultAsync();
                }
                index++;
                _logger.Info($"user.NickName: {user.NickName}");

                await UpdateUserImages(user);

            } while (user != null);
        }

        public async Task UpdateUserImages(PixelUser pixelUser, int page = 1)
        {
            while (true)
            {
                _logger.Info($"Page: {page}");
                var list = await _imagesParser.LoadImagesByUser(pixelUser.NickName, page);
                if (list is null || list.Count == 0)
                {
                    _logger.Info("No images.");
                    return;
                }
                _logger.Info($"Images loaded: {list.Count}");
                using (var pixelContext = new PixelContext(_dbContextOptions))
                {
                    foreach (var item in list)
                    {
                        var entity = await pixelContext.PixelImage
                            .Include(x=>x.PixelUser)
                            .SingleOrDefaultAsync(x => x.Url == item.imgUrl);
                        if (entity == null)
                        {
                            await pixelContext.PixelImage.AddAsync(new PixelImage {
                                PixelImageId=Guid.NewGuid(),
                                Url=item.imgUrl,
                                MetaUrl=item.metaUrl,
                                Title=item.title,
                                PixelUserId=pixelUser.PixelUserId
                            });
                        }
                        else
                        {
                            entity.Title = item.title;
                            entity.MetaUrl = item.metaUrl;
                            pixelContext.PixelImage.Update(entity);
                        }
                    }

                    await pixelContext.SaveChangesAsync();
                }

                page++;
            }
        }

        public async Task UpdateImagesMeta(int index = 0)
        {
            PixelImage pixelImage = null;
            do
            {
                _logger.Info($"index: {index}");
                using (var pixelContext = new PixelContext(_dbContextOptions))
                {
                    pixelImage = await pixelContext.PixelImage.Skip(index).FirstOrDefaultAsync();
                }
                index++;

                await UpdateImageMeta(pixelImage);

            } while (pixelImage != null);
        }

        public async Task UpdateImageMeta(PixelImage pixelImage)
        {
            _logger.Info($"metaurl: {pixelImage.MetaUrl}");
            var list = await _imageMetaParser.LoadImageTags(pixelImage.MetaUrl);
            if (list != null && list.Count > 0)
            {
                _logger.Info($"Tags loaded: {list.Count}");
                var keywords = string.Join(',', list);
                using (var pixelContext = new PixelContext(_dbContextOptions))
                {
                    pixelImage.Keywords = keywords;
                    pixelContext.PixelImage.Update(pixelImage);

                    await pixelContext.SaveChangesAsync();
                }
            }
            else
            {
                _logger.Info("No tags.");
            }

        }



        public async Task PixelImagesToGoogle()
        {
            PixelImage pixelImage = null;
            int skip = 0;
            do
            {
                _logger.Info($"skiping: {skip}");
                using (var pixelContext = new PixelContext(_dbContextOptions))
                {
                    var q = pixelContext.PixelImage
                        .Where(x => string.IsNullOrWhiteSpace(x.GoogleId)&&
                        x.ProcessState==null||x.ProcessState.Value==0)
                        .Skip(skip);
                    var count = await q.CountAsync();
                    _logger.Info($"To proccess: {count}");
                    pixelImage = await q.FirstOrDefaultAsync();
                }
                if (pixelImage ==null)
                {
                    break;
                }
                await PixelImageToGoogle(pixelImage);
            } while (true);
        }

        public async Task PixelImageToGoogle(PixelImage pixelImage)
        {

            _logger.Info($"url: {pixelImage.Url}");
            var arr = await _fileLoader.GetByteArrayAsync(pixelImage.Url);

            if (arr == null || arr.Length == 0)
            {
                _logger.Error("arr is null");
                using (var pixelContext = new PixelContext(_dbContextOptions))
                {
                    pixelImage.ProcessState = PixelImageProcessState.ImageNotFound;
                    pixelContext.Update(pixelImage);
                    await pixelContext.SaveChangesAsync();
                }
                return;
            }
            int width = 0;
            int height = 0;
            //string phashStr = null;
            using (var stream = new MemoryStream())
            {
                try
                {
                    using (var im = new MagickImage(arr))
                    {
                        im.Strip();
                        im.Format = MagickFormat.Jpeg;
                        im.Quality = 85;
                        im.Interlace = Interlace.Jpeg;
                        im.ColorSpace = ColorSpace.sRGB;
                        im.Settings.SetDefines(new JpegWriteDefines()
                        {
                            SamplingFactors = new MagickGeometry[] {
                                new MagickGeometry(4,2) }
                        });
                        width = im.Width;
                        height = im.Height;
                        im.Write(stream);
                        //im.Grayscale();
                        //var phash = im.PerceptualHash();
                        //phashStr = phash.ToString();
                    }
                }
                catch (ImageMagick.MagickErrorException ex) { 
                    _logger.Error(ex);
                    using (var pixelContext = new PixelContext(_dbContextOptions))
                    {
                        pixelImage.ProcessState= PixelImageProcessState.ImgFormatNotValid;
                        pixelContext.Update(pixelImage);
                        await pixelContext.SaveChangesAsync();
                    }
                    return;

                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    throw ex;
                }

                stream.Position = 0;

                try
                {
                    var name = string.IsNullOrEmpty(pixelImage.Title)
                        ? "image"
                        : pixelImage.Title;
                    var id = await _googleService.Upload(stream, $"{name}.jpg");

                    using (var pixelContext = new PixelContext(_dbContextOptions))
                    {
                        pixelImage.GoogleId = id;
                        //pixelImage.GrayscalePerceptualHash = phashStr;
                        pixelImage.Width = width;
                        pixelImage.Height = height;
                        pixelContext.Update(pixelImage);
                        await pixelContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    throw ex;
                }
            }
            return;
        }

        public async Task CleanGoogleImages() =>
            await _googleService.DeleteFiles(async x => {
                using var pixelContext = new PixelContext(_dbContextOptions);
                var pixelImage = await pixelContext.PixelImage.AnyAsync(y => y.GoogleId==x);
                return !pixelImage;
            });


        public async Task LoadMSVisionMeta()
        {
            List<PixelImage> pixelImages = null;
            int take = 1;
            int j=0;
            int skip = 0;
            List<PixelImage> list=null;
            using (var pixelContext = new PixelContext(_dbContextOptions))
            {
                var q = pixelContext.PixelImage
                    .Where(x => !string.IsNullOrWhiteSpace(x.GoogleId)
                    && string.IsNullOrWhiteSpace(x.MSVision)
                    && (x.ProcessState == null || x.ProcessState == PixelImageProcessState.None)
                    );
                list = await q.ToListAsync();
            }
            do
            {
                _logger.Info($"To proccess: {list.Count-skip}");
                _logger.Info($"Let's take: {take}");
                pixelImages = list.Skip(skip).Take(take).ToList();
                skip += take;
                if (pixelImages.Count==0)
                {
                    break;
                }
                var tasks = pixelImages.Select(x=> 
                    _msvisionService.Meta(x.Width > x.Height
                   ? $"https://drive.google.com/thumbnail?id={x.GoogleId}&sz=w2048"
                   : $"https://drive.google.com/thumbnail?id={x.GoogleId}&sz=h2048")).ToList();

                await Task.WhenAll(tasks);
                take = _msvisionService.ComputerVisionClients.Count*20;
                using (var pixelContext = new PixelContext(_dbContextOptions))
                {
                    for (int i = 0; i < pixelImages.Count; i++)
                    {
                        var result = tasks[i].Result;
                        if (result==null)
                        {
                            pixelImages[i].ProcessState= PixelImageProcessState.TooBigForGoogle;
                        }
                        else
                        {
                            var json = JsonConvert.SerializeObject(result);
                            pixelImages[i].MSVision = json;
                        }
                        pixelContext.Update(pixelImages[i]);
                    }

                    await pixelContext.SaveChangesAsync();
                }
                    j += pixelImages.Count;
                    _logger.Info($"saved to db: {j}");
            } while (true);
        }

        
    }
}
