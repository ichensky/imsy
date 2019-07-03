using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelCrawler.Services
{
    public class UserAgentService
    {
        static Random rnd = new Random();
        private string[] _userAgents;
        private NLog.Logger _logger;

        public UserAgentService(NLog.Logger logger, string[] userAgents)
        {
            _userAgents = userAgents;
            _logger = logger;
        }
        public string NewUserAgent() {
            int r = rnd.Next(_userAgents.Length);
            var userAgent= _userAgents[r];
            _logger.Info($"{nameof(userAgent)}:{userAgent}");
            return userAgent;
        }
    }
}
