using FileCloud.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.DataAccess.Configurations
{
    public class FileConfigurations : IEntityTypeConfiguration<FileEntitiy>
    {
        public void Configure(EntityTypeBuilder<FileEntitiy> builder)
        {
            builder.HasKey(x => x.id);
        }
    }
}
