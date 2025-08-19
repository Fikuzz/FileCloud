using FileCloud.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Core.Models
{
    public class File
    {
        private File(Guid id, string name, string path, long? size, Guid folderId)
        {
            this.Id = id;
            Name = name;
            Path = path;
            Size = size;
            FolderId = folderId;
        }
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long? Size { get; set; }
        public Guid FolderId { get; set; }

        public static Result<File> Create(Guid id, string name, string path, long? size, Guid folderId)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Result<File>.Fail("incorect file name");
            }

            var file = new File(id, name, path, size, folderId);

            return Result<File>.Success(file);
        }
    }
}
