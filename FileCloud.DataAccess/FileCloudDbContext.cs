using FileCloud.DataAccess.Configurations;
using FileCloud.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
namespace FileCloud.DataAccess
{
    public class FileCloudDbContext : DbContext
    {
        public FileCloudDbContext(DbContextOptions<FileCloudDbContext> options)
            : base(options) { }

        public DbSet<FolderEntity> Folders { get; set; }
        public DbSet<FileEntity> Files { get; set; }
        public DbSet<UserEntity> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new FolderConfiguration());
            modelBuilder.ApplyConfiguration(new FileConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfigurations());
        }
    }
}
