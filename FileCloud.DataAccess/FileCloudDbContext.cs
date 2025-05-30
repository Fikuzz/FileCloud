using FileCloud.DataAccess.Configurations;
using FileCloud.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
namespace FileCloud.DataAccess
{
    public class FileCloudDbContext : DbContext
    {
        public FileCloudDbContext(DbContextOptions<FileCloudDbContext> options)
            : base(options)
        {
        }

        public DbSet<FileEntitiy> Files { get; set; }
    }
}
