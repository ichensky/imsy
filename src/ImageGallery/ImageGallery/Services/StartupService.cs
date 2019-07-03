using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageGallery.Config;
using ImageGalleryDb.Models.ImageGallery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog;

namespace ImageGallery.Services
{
    public class StartupService
    {
        private DbContextOptions options;

        public NLog.Logger Logger { get; }
        public Config.Config Config { get; }
        public Data Data { get; }
        public ILoggerFactory LoggerFactory { get; set; }

        public StartupService(NLog.Logger logger,
                              Config.Config config,
                              Data data)
        {
            Logger = logger;
            Config = config;
            Data = data;
        }

    }
}
