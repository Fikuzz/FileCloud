using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FileCloud.DataAccess.Entities;

namespace FileCloud.DataAccess.Configurations;
public class FolderConfiguration : IEntityTypeConfiguration<FolderEntity>
{
    public void Configure(EntityTypeBuilder<FolderEntity> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasMany(f => f.SubFolders)
            .WithOne(f => f.Parent)
            .HasForeignKey(f => f.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.Files)
            .WithOne(f => f.Folder)
            .HasForeignKey(f => f.FolderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Owner)
            .WithMany(u => u.Folders)
            .HasForeignKey(f => f.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => new { f.ParentId, f.Name })
            .IsUnique();
    }
}