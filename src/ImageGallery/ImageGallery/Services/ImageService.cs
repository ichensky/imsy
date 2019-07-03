using ImageGallery.Config;
using ImageGalleryDb;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.Services
{
    public class ImageService
    {
        private readonly Random _random = new Random();
        private NLog.Logger _logger;
        private readonly Data _data;
        public ImageService(NLog.Logger logger, Data data) {
            this._logger = logger;
            this._data=data;

        }
        public async Task<string> Random(int h = 220)
        {
            if (h < 1 && h > 2048)
            {
                return null;
            }

            var id = _random.Next(0, _data.ImagesCount-1);
            using var imContext = new ImContext(_data.Options);
            var gid = (await imContext.Image.Skip(id).FirstOrDefaultAsync()).GoogleDriveId;

            return $"https://drive.google.com/thumbnail?id={gid}&sz=h{h}";
        }
    }
}
