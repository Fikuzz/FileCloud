using FileCloud.Core;
using FileCloud.Core.Models;
using FileCloud.DataAccess.Entities;

namespace FileCloud.DataAccess.Mappers
{
    public static class FolderMapper
    {
        public static Result<Folder> ToModel(this FolderEntity entity)
        {
            var subFolders = (entity.SubFolders ?? Enumerable.Empty<FolderEntity>())
                .Select(folder =>
                {
                    var result = Folder.Create(folder.Id, folder.Name, folder.ParentId, null, null);
                    return result.IsSuccess ? result.Value : null;
                })
                .Where(f => f != null)
                .ToList();

            var files = (entity.Files ?? Enumerable.Empty<FileEntity>())
                .Select(file =>
                {
                    var result = Core.Models.File.Create(file.Id, file.Name, string.Empty, null, file.FolderId);
                    return result.IsSuccess ? result.Value : null;
                })
                .Where(f => f != null)
                .ToList();

            return Folder.Create(entity.Id, entity.Name, entity.ParentId, subFolders, files);
        }
    }
}
