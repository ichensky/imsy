using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using PixelCrawler.Helpers;

namespace PixelCrawler.Services
{
    public class GoogleService
    {
        private NLog.Logger _logger;
        private List<string> _keys;

        public GoogleService(NLog.Logger logger, string pathToKeys) {
            this._logger = logger;
            _keys = Directory.EnumerateFiles(pathToKeys)
                .Select(x => File.ReadAllText(x)).ToList();
        }


        private async Task<GoogleDriveService> GoogleDriveService(long memory) {
            begin:
            var key = _keys.FirstOrDefault();
            if (key == null)
            {
                var msg = "No avalible keys.";
                _logger.Error(msg);
                throw new Exception(msg);
            }
            var gd = new GoogleDriveService(_logger, key);

            var space = await gd.AvailableSpaceGoogleDrive();
            _logger.Info($"There are {(float)space/(1024*1024)} avalible mb at google drive.");
            if (space < memory + 10 * 1024 * 1024) // memory + additional 10mb
            {
                _keys.RemoveAt(0);
                goto begin;
            }

            return gd;
        }

        public async Task AvailableSpace(GoogleDriveService gd, Action action) {
            var spaceBefore = await gd.AvailableSpaceGoogleDrive();
            _logger.Info($"Avalible: {(float)spaceBefore / (1024 * 1024)} mb.");
            action();
            var spaceAfter = await gd.AvailableSpaceGoogleDrive();
            var cleaned = spaceAfter - spaceBefore ;
            _logger.Info($"Cleaned: {cleaned} b.");
            _logger.Info($"Avalible: {(float)spaceAfter / (1024 * 1024)} mb.");
        }

        public async Task DeleteFiles(Func<string, Task<bool>> IsDeleteFile)
        {
            foreach (var key in _keys)
            {
                var gd = new GoogleDriveService(_logger, key);
                var ids = (await gd.ListAllFiles()).Select(x=>x.Id);
                var count = ids.Count();
                _logger.Info($"There are `{count}` ids.");
                int i = 0;
                int j = 0;
                foreach (var id in ids)
                {
                    if (await IsDeleteFile(id))
                    {
                        await gd.Files.Delete(id).ExecuteAsync();
                        j++;
                    }
                    i++;
                }
            }
        }



        public async Task<string> Upload(Stream stream, string imageName) {

            using(var gd = await GoogleDriveService(stream.Length)) {
                var id = await gd.UploadToGoogle(stream, imageName);
               
                begin:
                try
                {
                    await gd.AddPermissionsAnyoneReader(id);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    goto begin;
                }
                return id;
            }
        }
    }
}
