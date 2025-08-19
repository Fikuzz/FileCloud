using FileCloud.Core;
using FileCloud.DataAccess.Entities;
using Model = FileCloud.Core.Models;

namespace FileCloud.DataAccess.Mappers
{
    public static class FileMapper
    {
        public static Result<Model.File> ToModel(this FileEntity f) =>
            Model.File.Create(f.Id, f.Name, f.Path, f.Size, f.FolderId);
    }
}
