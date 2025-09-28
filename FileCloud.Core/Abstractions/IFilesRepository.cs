using System.Reflection;

namespace FileCloud.Core.Abstractions
{
    public interface IFilesRepository
    {
        Task<Guid> Create(Core.Models.File file);
        Task<Guid> Delete(Guid id);
        Task<List<Result<Core.Models.File>>> GetAll(Guid userId);
        Task<List<Result<Core.Models.File>>> GetChild(Guid id);
        Task<Result<Core.Models.File>> Get(Guid id, Guid userId);
        Task<Guid> Rename(Guid id, string name);
        Task<Guid> Move(Guid id, string path, Guid folderId);
        Task<Guid> Size(Guid id, long size);
    }
}