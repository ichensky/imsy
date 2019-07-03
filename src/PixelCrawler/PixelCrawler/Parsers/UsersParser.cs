using PixelCrawler.Helpers;
using PixelCrawler.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime;
using HtmlAgilityPack;

namespace PixelCrawler.Parsers
{
    public class UsersParser
    {
        private NLog.Logger _logger;
        private HttpClientService _httpClientService;
        private AttemptService _attemptService;

        public UsersParser(NLog.Logger logger, HttpClientService httpClientService, AttemptService attemptService)
        {
            _logger = logger;
            _httpClientService = httpClientService;
            _attemptService = attemptService;
        }

        public async Task<List<(string nickName, string userName)>> LoadUsersByPage(int page)
        {
            _logger.Info($"{nameof(page)}: {page}");


            var url = $"https://www.pexels.com/leaderboard/all-time/?page={page}";
            var str = await _attemptService.Operation(async () => {
                using (var client = _httpClientService.New())
                {
                   return await _httpClientService.GetStringAsync(url);
                }
            }, 5);

            var doc = new HtmlDocument();

            try
            {
                doc.LoadHtml(str);
                return doc.DocumentNode
               .SelectNodes("//body/section/div/article/div/div/h3/a")
               .Select(x => (x.Attributes["href"].Value.TrimStart('/').Trim(), x.InnerText.Trim()))
               .ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return null;           
        }
      
    }
}
