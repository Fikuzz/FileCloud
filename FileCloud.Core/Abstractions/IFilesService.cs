using FileCloud.Core.Models;

namespace FileCloud.Core.Abstractions
{
    public interface IFilesService
    {
        Task<List<Core.Models.File>> GetAllFiles();
        Task<Guid> UpdateFile(Guid id, string name, string path);
        Task<Guid> UploadFile(Core.Models.File file);
        Task<Guid> DeleteFile(Guid id);
        Task<Core.Models.File> GetFileWithId(Guid id);
    }
}