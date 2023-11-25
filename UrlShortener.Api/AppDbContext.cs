using Microsoft.EntityFrameworkCore;
using System.Data;
using URLShortenerApi.Entities;
using URLShortenerApi.Services;

namespace UrlShortener.Api
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<ShortenedUrl> ShortenedUrls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ShortenedUrl>(builder =>
            {
                builder.Property(s => s.Code).HasMaxLength(UrlShorteningService.NumberOfCharsInShortLink);
                builder.HasIndex(s => s.Code).IsUnique();
            });
        }
    }
}
