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
        private Folder(Guid id, string name, Guid? parentId, ICollection<Guid> subFoldersId, ICollection<Guid> filesId)
        {
            Id = id;
            Name = name;
            ParentId = parentId;
            SubFoldersId = subFoldersId;
            FilesId = filesId;
        }

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        // Родительская папка
        public Guid? ParentId { get; set; }
        // Дочерние папки
        public ICollection<Guid> SubFoldersId { get; set; } = new List<Guid>();
        // Файлы в папке
        public ICollection<Guid> FilesId { get; set; } = new List<Guid>();

        public static Result<Folder> Create(Guid id, string name, Guid? parentId, ICollection<Guid> subFoldersId, ICollection<Guid> filesId)
        {
            if(string.IsNullOrEmpty(name))
            {
                return Result<Folder>.Fail("incorect folder name");
            }
            var folder = new Folder(id, name, parentId, subFoldersId, filesId);
            return Result<Folder>.Success(folder);
        }
    }
}
