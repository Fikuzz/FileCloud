using FileCloud.Core;
using FileCloud.Core.Models;
using FileCloud.DataAccess.Entities;

namespace FileCloud.DataAccess.Mappers
{
    public static class FolderMapper
    {
        public static Result<Folder> ToModel(this FolderEntity entity) =>
        Folder.Create(entity.Id, entity.Name, entity.ParentId,
            entity.SubFolders.Select(f => f.Id).ToList(),
            entity.Files.Select(f => f.Id).ToList());
    }
}
