using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileCloud.Core.Abstractions;

namespace FileCloud.Core.Models
{
    public class Folder
    {
        private Folder(Guid id, string name, Guid? parentId, Guid ownerId)
        {
            Id = id;
            Name = name;
            ParentId = parentId;
            OwnerId = ownerId;
        }

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public bool IsRoot { get; set; } = false;
        public Guid OwnerId { get; set; }
        // Родительская папка
        public Guid? ParentId { get; set; }
        // Дочерние папки
        public ICollection<Folder> SubFolders { get; set; } = new List<Folder>();
        // Файлы в папке
        public ICollection<File> Files { get; set; } = new List<File>();
        public static Result<Folder> Create(Guid id, string name, Guid? parentId, Guid ownerId, ICollection<Folder> folders = null, ICollection<File> files = null)
        {
            if(string.IsNullOrEmpty(name))
            {
                return Result<Folder>.Fail("incorect folder name");
            }
            var folder = new Folder(id, name, parentId, ownerId);
            folder.SubFolders = folders;
            folder.Files = files;
            return Result<Folder>.Success(folder);
        }
    }
}
