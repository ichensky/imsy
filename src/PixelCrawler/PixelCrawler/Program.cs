using EntityFrameworkCore.Sqlite.Migrations;
using ImageGalleryDb;
using ImageGalleryDb.Models;
using Microsoft.EntityFrameworkCore;
using NLog;
using PixelCrawler.Parsers;
using PixelCrawler.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace PixelCrawler
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var logPath = @"E:\proj\my\ImageGallery\log\pexel";

            var pixel_dbPath = @"E:\proj\my\ImageGallery\pixel.db";

            //var im_dbPath = @"E:\proj\my\ImageGallery\im.db";
            var im_dbPath = @"E:\proj\my\ImageGallery\test.im.db";

            //var jsonDbPath= @"E:\proj\my\ImageGallery\db.json";
            var jsonDbPath= @"E:\proj\my\ImageGallery\test.db.json";


            var userAgentsPath = @"E:\proj\github\ichensky\ImageGallery\data\useragents.txt";
            var pixelMigrationsPath = @"E:\proj\github\ichensky\ImageGallery\src\db\pixel.txt";
            var imMigrationsPath = @"E:\proj\github\ichensky\ImageGallery\src\db\im.txt";

            var pathToGoogleKeys = @"E:\proj\github\ichensky\ImageGallery\keys\google";
            var msVisionKeysPath = @"E:\proj\github\ichensky\ImageGallery\keys\msvisionkeys.txt";

            LogManager.Configuration = LoggerService.DefaultConfiguration(logPath);
            var logger = LogManager.GetLogger(Services.Logger.log.ToString());

            var userAgents = await File.ReadAllLinesAsync(userAgentsPath);
            var userAgentsService = new UserAgentService(logger, userAgents);

            var stopwatchService = new StopwatchService(logger);
            var attemptService = new AttemptService(logger);

            var httpClientSerive = new HttpClientService(logger, stopwatchService, userAgentsService);
            var usersParser = new UsersParser(logger, httpClientSerive, attemptService);
            var imagesParser = new ImagesParser(logger, httpClientSerive, attemptService);
            var imageMetaParser = new ImageMetaParser(logger, httpClientSerive, attemptService);
            var fileLoader = new FileLoader(logger, httpClientSerive, attemptService);

            var googleService = new GoogleService(logger, pathToGoogleKeys);

            var msvisionService = new MSVisionService(logger,msVisionKeysPath);

            var pixel_builder = new DbContextOptionsBuilder().UseSqlite($"Data Source={pixel_dbPath}");

            var imBuilder = new DbContextOptionsBuilder().UseSqlite($"Data Source={im_dbPath}");
          

            //await googleService.Clean();

            //var migrationService = new MigrationsService(pixel_builder.Options);
            //logger.Info("Applying migrations to database.");
            //await migrationService.MigrateAsync(migrationsPath);

            var pixelService = new PixelService(logger, pixel_builder.Options, 
                usersParser, imagesParser, imageMetaParser,
                fileLoader, googleService, msvisionService);
            //await pixelService.UpdateUsers(5514);
            //await pixelService.UpdateUsersImages(16124);
            //await pixelService.UpdateImagesMeta(388970);

            //await pixelService.PixelImagesToGoogle();
            //await pixelService.CleanGoogleImages();

            //--
            //--
            //await pixelService.LoadMSVisionMeta();
            //--
            //--
            //var jsonDb = await pixelService.GenJsonDb();
            //File.WriteAllText(jsonDbPath, jsonDb);
            //--
            //--




            if (File.Exists(im_dbPath))
            {
                var err = "Deleting database file ...";
                logger.Warn(err);
                File.Delete(im_dbPath);
            }
            var migrationService = new MigrationsService(imBuilder.Options);
            logger.Info("Applying migrations to database.");
            await migrationService.MigrateAsync(imMigrationsPath);



            //var im_connection = new Microsoft.Data.Sqlite.SqliteConnection($"DataSource={im_dbPath}");
            //im_connection.Open();
            //var connectionInMemory = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
            //connectionInMemory.Open();
            //var builderInMemory = new DbContextOptionsBuilder().UseSqlite(connectionInMemory);
            //im_connection.BackupDatabase(connectionInMemory);

            var pixelToImageGalleryService = new PixelToImageGalleryService(
                logger, pixel_builder.Options,
                imBuilder.Options);

            //builderInMemory.Options);

            //--
            //var jsonDb = await pixelToImageGalleryService.GenJsonDb();
            //File.WriteAllText(jsonDbPath, jsonDb);
            //--

            var json = File.ReadAllText(jsonDbPath);
            await pixelToImageGalleryService.GenDb(json);
            //connectionInMemory.BackupDatabase(im_connection);

            Console.WriteLine("Hello World!");
        }
    }
}
