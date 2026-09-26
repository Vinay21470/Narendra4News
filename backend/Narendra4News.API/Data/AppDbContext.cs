using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Narendra4News.API.Models;
namespace Narendra4News.API.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options) {
 public DbSet<Article> Articles => Set<Article>(); public DbSet<Movie> Movies => Set<Movie>(); public DbSet<Category> Categories => Set<Category>(); public DbSet<CollectionRecord> Collections => Set<CollectionRecord>(); public DbSet<Comment> Comments => Set<Comment>(); public DbSet<MediaItem> Media => Set<MediaItem>();
 protected override void OnModelCreating(ModelBuilder b) { base.OnModelCreating(b); b.Entity<Article>().HasIndex(x=>x.Slug).IsUnique(); b.Entity<Movie>().HasIndex(x=>x.Slug).IsUnique(); b.Entity<Category>().HasIndex(x=>x.Slug).IsUnique(); b.Entity<CollectionRecord>().HasIndex(x=>new{x.MovieId,x.DayNumber,x.CollectionDate}).IsUnique(); b.Entity<Article>().HasIndex(x=>new{x.Status,x.PublishedDate}); b.Entity<Article>().HasIndex(x=>new{x.MovieId,x.PublishedDate}); b.Entity<Article>().Property(x=>x.Status).HasConversion<string>(); b.Entity<Comment>().Property(x=>x.Status).HasConversion<string>(); b.Entity<Movie>().Property(x=>x.Budget).HasPrecision(18,2); foreach(var name in new[]{"IndiaNet","IndiaGross","Overseas","WorldwideGross","OpeningDay","WeekendCollection","TotalCollection"}) b.Entity<CollectionRecord>().Property<decimal?>(name).HasPrecision(18,2); }
}
