using FileCloud.DataAccess.Configurations;
using FileCloud.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
namespace FileCloud.DataAccess
{
    public class FileCloudDbContext : DbContext
    {
        public static readonly Guid RootFolderId =
            new Guid("11111111-1111-1111-1111-111111111111");
        public FileCloudDbContext(DbContextOptions<FileCloudDbContext> options)
            : base(options) { }

        public DbSet<FolderEntity> Folders { get; set; }
        public DbSet<FileEntity> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new FolderConfiguration());
            modelBuilder.ApplyConfiguration(new FileConfiguration());

            modelBuilder.Entity<FolderEntity>().HasData(
            new FolderEntity
            {
                Id = RootFolderId,
                Name = "Root",
                ParentId = null
            });
        }
    }
}
