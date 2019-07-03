using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageGallery.Controllers;
using ImageGallery.Logger;
using ImageGallery.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using S.NET;

namespace ImageGallery
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private StartupService _startupService;

        public Startup(IConfiguration configuration, StartupService startupService)
        {
            Configuration = configuration;
            this._startupService = startupService;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var logger = _startupService.Logger;
            var loggerFactory = _startupService.LoggerFactory ;

            services
            .AddSingleton(loggerFactory)
            .AddLogging()
            .AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMemoryCache();
            //var memoryCache = new MemoryCache(new MemoryCacheOptions { });


            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddResponseCaching();













            //services.AddCors(options =>
            //{
            //    options.AddDefaultPolicy(
            //        builder => builder
            //        .AllowAnyOrigin()
            //        .AllowAnyMethod()
            //        .AllowAnyHeader()
            //        .AllowCredentials());
            //});
            services.AddMvc()
                .AddNewtonsoftJson(options => {
                options.SerializerSettings.NullValueHandling
            = Newtonsoft.Json.NullValueHandling.Ignore;
            })
            .AddControllersAsServices();

            var imageService = new ImageService(logger, _startupService.Data);

            services.AddSingleton(x => new GalleryController(logger, _startupService.Data, imageService));
            services.AddSingleton(x => new ImageController(logger, _startupService.Data));
            services.AddSingleton(x => new HomeController(logger, _startupService.Data, imageService));
            services.AddSingleton(x => new TagsController(logger, _startupService.Data));
            services.AddSingleton(x => new CategoriesController(logger, _startupService.Data));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseExceptionHandler("/error/404");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseStatusCodePagesWithReExecute("/error/{0}");


            app.UseHttpsRedirection();
            app.UseCookiePolicy();

            app.UseResponseCaching();
            app.UseDefaultFiles();
            //var rewrite = new RewriteOptions()
            //  .AddRewrite("image/?id=", "images/", true)
            //  //.AddRewrite("index.html?id=", "index.html", true)
            //  //.AddRewrite("index.html?id=", "index.html", true)
            //  ;

            //app.UseRewriter(rewrite);
            app.UseStaticFiles();

            //app.MapWhen(context =>
            //{
            //    var path = context.Request.Path.Value;
            //    return path.StartsWith("/index", StringComparison.OrdinalIgnoreCase) ||
            //           path.StartsWith("/search", StringComparison.OrdinalIgnoreCase);
            //}, config => config.UseStaticFiles());
            //app.UseCors();
            app.UseMvc();

            //app.UseMvc(routes => routes.MapRoute("default", "{controller}/{action}"));
        }
    }
}
