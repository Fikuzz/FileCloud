using FileCloud.Core.Models;
using FileCloud.DataAccess.Entities;

namespace FileCloud.DataAccess.Mappers
{
    public static class FolderMapper
    {
        public static (Folder Folder, string Error) ToModel(this FolderEntity entity) =>
        Folder.Create(entity.Id, entity.Name, entity.ParentId,
            entity.SubFolders.Select(f => f.Id).ToList(),
            entity.Files.Select(f => f.Id).ToList());
    }
}
