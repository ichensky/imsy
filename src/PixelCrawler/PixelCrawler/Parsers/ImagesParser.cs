using HtmlAgilityPack;
using PixelCrawler.Helpers;
using PixelCrawler.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static System.Net.Mime.MediaTypeNames;

namespace PixelCrawler.Parsers
{
    public class ImagesParser
    {
        private NLog.Logger _logger;
        private HttpClientService _httpClientService;
        private AttemptService _attemptService;

        public ImagesParser(NLog.Logger logger, HttpClientService httpClientService, AttemptService attemptService) {
            _logger = logger;
            _httpClientService = httpClientService;
            _attemptService = attemptService;
        }

        public async Task<List<(string imgUrl, string metaUrl, string title)>> LoadImagesByUser(string user, int page = 1)
        {
            _logger.Info($"{nameof(user)}: {user}");

            var url = $"https://www.pexels.com/{user}.js?page={page}&seed=&format=js";
            var str = await _attemptService.Operation(async () =>
            {
                using (var client = _httpClientService.New())
                {
                    return await _httpClientService.GetStringAsync(url);
                }
            }
            , 5);

            if (str is null)
            {
                _logger.Info($"str is null");
                return null;
            }

            List<(string imgUrl, string metaUrl, string title)> images =null;

            var ba = str.IndexOf("infiniteScrollingAppender.append('");
            if (ba < 0)
            {
                return null;
            }
            var ea = str.IndexOf("infiniteScrollingAppender.execute()");
            if (ea < 0)
            {
                return null;
            }
            str = str.Remove(ea).Remove(0, ba)
                .Replace("\\/", "/")
                .Replace("\\\"", @"""")
                .Replace("\\'", @"""")
                .Replace("infiniteScrollingAppender.append('", "");
            str = Regex.Replace(str, "', [0-9]+[)];", "");

            var doc = new HtmlDocument();
            try
            {
                doc.LoadHtml(str);
                var nodes = doc.DocumentNode
                    .SelectNodes("/div/article/a[@class='js-photo-link photo-item__link']");
                if (nodes != null)
                {
                    images = new List<(string imgUrl, string metaUrl, string title)>();
                    foreach (var node in nodes)
                    {
                        var (metaUrl, title) = ($"https://www.pexels.com{node.Attributes["href"].Value.Trim()}",
                            node.Attributes["title"]?.Value.Trim());
                        var img = node.SelectSingleNode("./img").Attributes["src"].Value;
                        var index = img.IndexOf('?');
                        if (index > 0)
                        {
                            var imgUrl = img.Remove(index).TrimStart();
                            images.Add((imgUrl, metaUrl, title));
                        }
                        else
                        {
                            throw new Exception("No image");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return images;
        }

    }
}
