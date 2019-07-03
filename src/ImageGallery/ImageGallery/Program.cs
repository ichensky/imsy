using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageGallery.Config;
using ImageGallery.Logger;
using ImageGallery.Models.Gallery;
using ImageGallery.Services;
using ImageGalleryDb;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Extensions.Logging;
using S.NET;
using SomeApplicationNamespace;
using StackExchange.Profiling;

namespace SomeApplicationNamespace
{
    using Microsoft.EntityFrameworkCore.Internal;
    using Microsoft.EntityFrameworkCore.Query;
    using Microsoft.EntityFrameworkCore.Query.Internal;
    using Microsoft.EntityFrameworkCore.Storage;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    public static class IQueryableHelper
    {

        private static readonly FieldInfo _queryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.Single(x => x.Name == "_queryCompiler");

        private static readonly TypeInfo _queryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();

        private static readonly FieldInfo _queryModelGeneratorField = _queryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_queryModelGenerator");

        private static readonly FieldInfo _databaseField = _queryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");

        private static readonly PropertyInfo _dependenciesProperty = typeof(Database).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Dependencies");

        public static string ToSql<TEntity>(this IQueryable<TEntity> queryable)
            where TEntity : class
        {
            if (!(queryable is EntityQueryable<TEntity>) && !(queryable is InternalDbSet<TEntity>))
                throw new ArgumentException();

            var queryCompiler = (IQueryCompiler)_queryCompilerField.GetValue(queryable.Provider);
            var queryModelGenerator = (IQueryModelGenerator)_queryModelGeneratorField.GetValue(queryCompiler);
            var queryModel = queryModelGenerator.ParseQuery(queryable.Expression);
            var database = _databaseField.GetValue(queryCompiler);
            var queryCompilationContextFactory = ((DatabaseDependencies)_dependenciesProperty.GetValue(database)).QueryCompilationContextFactory;
            var queryCompilationContext = queryCompilationContextFactory.Create(false);
            var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();
            modelVisitor.CreateQueryExecutor<TEntity>(queryModel);
            return modelVisitor.Queries.Join(Environment.NewLine + Environment.NewLine);
        }
    }
}

namespace ImageGallery
{
  
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var settingsData = File.ReadAllText("settings.clj");
            var config = SConvert.DeserializeObject<Config.Config>(settingsData);

            LoggerService.Init(config.LogPath);
            var logger = LogManager.GetLogger(Logger.Logger.log.ToString());
            var loggerFactory = ((ILoggerFactory)(new LoggerFactory())).AddNLog();
            
            //var xloggerFactory = ((ILoggerFactory)(LoggerFactory.Create(x=>x.AddConsole())));

            logger.Info("App starting ...");
            var data = new Data
            {
                //Options = options
            };


            var dataSource = $"DataSource={config.DbPath}";

            var profiler = MiniProfiler.StartNew("My Pofiler Name");

            using (var connection = new Microsoft.Data.Sqlite.SqliteConnection(dataSource))
            {
                connection.Open();

                var connectionInMemory = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
                connectionInMemory.Open();

                var builderInMemory = new DbContextOptionsBuilder().UseSqlite(connectionInMemory);
                data.Options = builderInMemory.Options;
                connection.BackupDatabase(connectionInMemory);
            }

            //var connection = new Microsoft.Data.Sqlite.SqliteConnection(dataSource);
            //var connectionInMemoryProfile = new StackExchange.Profiling.Data.ProfiledDbConnection(connection, MiniProfiler.Current);
            //var dbContextOptionsBuilder = new DbContextOptionsBuilder().UseSqlite(connectionInMemoryProfile);
            //var options = dbContextOptionsBuilder.Options;


            using (var imContext = new ImContext(data.Options))
            {
                data.TopTags = await imContext.Str
                    .Where(x => x.TopScoreMin > 0.5)
                    .OrderByDescending(x => x.TopScoreMin)
                    .Select(x => x.Name)
                    .ToListAsync();

                var categories = await imContext.Category
                    .Select(x => x.Name)
                    .ToListAsync();


                data.SortedCategories = categories
                    .Where(x => x.IndexOf('_') == x.Length - 1)
                    .Select(x => x.TrimEnd('_'))
                    .AsParallel()
                    .OrderBy(x => x)
                    .Select(x => new Category
                    {
                        Id = string.Intern(x + "_"),
                        Name = x,
                        Categories = categories
                            .Where(y => y.IndexOf(x) == 0 && y.Length > x.Length + 1)
                            .Select(y => new Category
                            {
                                Id = string.Intern(y),
                                Name = y.Replace("_", " ").Remove(0, x.Length + 1)
                            })
                                .OrderBy(x => x.Name)
                            .ToList()
                    })
                    .ToList();
                data.ImagesCount = await imContext.Image.CountAsync();
                data.Colors = await imContext.ImageStr
                    .Where(x => x.IsDominantColor == ImageGalleryDb.Models.Im.Boolean.True)
                    .Select(x => x.Str.Name)
                    .Distinct()
                    .ToListAsync();


                //var images = await imContext.Image.ToListAsync();
                //var sitemapService = new SitemapService(logger, images, config.Domain);
                //// TODO: comment this code
                //var path = Path.Combine(Environment.CurrentDirectory, "wwwroot", "sitemap");
                //var fileNames = await sitemapService.ReGenerate(path);
                //fileNames.ForEach(x => Console.WriteLine(x));

            }



            var startupService = new StartupService(logger,config, data);
            startupService.LoggerFactory = loggerFactory;
            CreateWebHostBuilder(args, startupService).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder
            (string[] args, StartupService startupService) =>
            WebHost.CreateDefaultBuilder(args)
            .CaptureStartupErrors(true)
            .ConfigureServices(x => x.AddSingleton(startupService))
                .UseStartup<Startup>();
    }
}
