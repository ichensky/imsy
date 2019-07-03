using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ImageGalleryDb.Models.Im;
using Microsoft.Extensions.Logging;

namespace ImageGalleryDb
{
    public class ImContext : DbContext
    {
        public DbSet<Image> Image { get; set; }
        public DbSet<Str> Str { get; set; }
        public DbSet<ImageStr> ImageStr { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<ImageCategory> ImageCategory { get; set; }
        public DbSet<DescriptionCaption> DescriptionCaption { get; set; }
        public DbSet<ImageDescriptionCaption> ImageDescriptionCaption { get; set; }
        public ImContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
