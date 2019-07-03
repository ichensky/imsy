using HtmlAgilityPack;
using PixelCrawler.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelCrawler.Parsers
{
    public class ImageMetaParser
    {
        private NLog.Logger _logger;
        private HttpClientService _httpClientService;
        private AttemptService _attemptService;

        public ImageMetaParser(NLog.Logger logger, HttpClientService httpClientService, AttemptService attemptService)
        {
            _logger = logger;
            _httpClientService = httpClientService;
            _attemptService = attemptService;
        }

        public async Task<List<string>> LoadImageTags(string metaUrl)
        {
            var str = await _attemptService.Operation(async () => {
                using (var client = _httpClientService.New())
                {
                    return await _httpClientService.GetStringAsync(metaUrl);
                }
            }, 5);

            if (str != null)
            {
                var doc = new HtmlDocument();
                try
                {
                    doc.LoadHtml(str);
                    return doc.DocumentNode
                        .SelectNodes("//body/div/div/section/div/div/ul/li/a")
                        .Select(x => x.InnerText.Trim())
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
            return null;
        }
    }
}
