using ImageGalleryDb.Models;
using ImageGalleryDb.Models.Pixel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImageGalleryDb
{
    public class PixelContext : DbContext
    {
        public DbSet<PixelUser> PixelUser { get; set; }
        public DbSet<PixelImage> PixelImage { get; set; }
        public PixelContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
