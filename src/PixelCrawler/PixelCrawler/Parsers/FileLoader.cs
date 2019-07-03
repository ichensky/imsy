using HtmlAgilityPack;
using ImageMagick;
using PixelCrawler.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelCrawler.Parsers
{
    public class FileLoader
    {
        private NLog.Logger _logger;
        private HttpClientService _httpClientService;
        private AttemptService _attemptService;

        public FileLoader(NLog.Logger logger, HttpClientService httpClientService, AttemptService attemptService)
        {
            _logger = logger;
            _httpClientService = httpClientService;
            _attemptService = attemptService;
        }

        public async Task<byte[]> GetByteArrayAsync(string url)
        {
            return await _attemptService.Operation(async () => {
                using (var client = _httpClientService.New()) {
                    return await _httpClientService.GetByteArrayAsync(url);
                }
            }, 5);
        }
    }
}
