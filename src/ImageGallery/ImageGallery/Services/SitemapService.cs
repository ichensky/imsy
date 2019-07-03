using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageGalleryDb.Models.Im;
using NLog;

namespace ImageGallery.Services
{
    public class SitemapService
    {
        private const int SiteMapCount = 45000;
        private NLog.Logger _logger;
        private List<Image> _images;
        private string _domain;

        public SitemapService(NLog.Logger logger, List<Image> images, string domain) {

            _logger = logger;
            _images = images;
            _domain=domain;
        }
        public async Task<List<string>> ReGenerate(string path) {

            Directory.EnumerateFiles(path).ToList()
                .ForEach(x=>File.Delete(x));

            var count = Math.Ceiling((decimal)_images.Count / SiteMapCount);
            var fileNames = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var urls = _images.Skip(i * SiteMapCount).Take(SiteMapCount)
                    .Select(x => x.SeoId)
                    .Select(x => new Uri(new Uri(_domain),$"image/{x}").ToString())
                    ;
                var fileFullName = NewFileFullName(path);
                await File.WriteAllLinesAsync(fileFullName,urls);
                fileNames.Add(Path.GetFileName(fileFullName));
            }
            var root = Path.Combine(path, "root_"+Guid.NewGuid().ToString("N") + ".txt");
            var rootUrls = fileNames
                    .Select(x => new Uri(new Uri(_domain), $"sitemap/{x.Replace(path+"/","")}").ToString());

            await File.WriteAllLinesAsync(root, rootUrls);
            return fileNames;
        }
        private string NewFileFullName(string path) =>Path.Combine(path, Guid.NewGuid().ToString("N")+".txt");
    }
}
