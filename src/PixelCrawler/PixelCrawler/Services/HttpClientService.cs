using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PixelCrawler.Services
{
    public class HttpClientService 
    {
        private StopwatchService _stopwatchService;
        private NLog.Logger _logger;
        private UserAgentService _userAgentService;
        private HttpClient _client;

        public HttpClientService(NLog.Logger logger, StopwatchService stopwatchService, UserAgentService userAgentService)
        {
            _stopwatchService = stopwatchService;
            _logger = logger;
            _userAgentService = userAgentService;

        }
        public HttpClient New() {
            _client= new HttpClient();
            _client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            _client.DefaultRequestHeaders.Add("UserAgent", _userAgentService.NewUserAgent());
            return _client;
        }
        
        public async Task<string> GetStringAsync(string url) {
            _logger.Info($"{nameof(url)}: {url}");
            return await GetDataAsync(async () => await _client.GetStringAsync(url));
        }
        public async Task<Stream> GetStreamAsync(string url)
        {
            _logger.Info($"{nameof(url)}: {url}");
            return await GetDataAsync(async () => await _client.GetStreamAsync(url));
        }
        public async Task<byte[]> GetByteArrayAsync(string url)
        {
            _logger.Info($"{nameof(url)}: {url}");
            return await GetDataAsync(async () => await _client.GetByteArrayAsync(url));
        }

        private async Task<T> GetDataAsync<T>(Func<Task<T>> func)
        {
            var str = await _stopwatchService.Operation<T>(async () =>
            {
                try
                {
                    return await func();
                }
                catch (HttpRequestException ex)
                {
                    if (ex.Message.Contains("500") || ex.Message.Contains("404"))
                    {
                        _logger.Error(ex);
                        return default;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            );
            return str;
        }
    }
}
