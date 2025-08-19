using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FileCloud.DataAccess.Entities;

namespace FileCloud.DataAccess.Configurations;
public class FileConfiguration : IEntityTypeConfiguration<FileEntity>
{
    public void Configure(EntityTypeBuilder<FileEntity> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(f => f.Path)
            .IsRequired();

        builder.Property(f => f.Size)
            .IsRequired(false);

        builder.HasOne(f => f.Folder)
            .WithMany(f => f.Files)
            .HasForeignKey(f => f.FolderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => new { f.FolderId, f.Name })
            .IsUnique();
    }
}