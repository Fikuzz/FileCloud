using System.Reflection;

namespace FileCloud.Core.Abstractions
{
    public interface IFilesRepositories
    {
        Task<Guid> Create(Core.Models.File file);
        Task<Guid> Delete(Guid id);
        Task<List<Result<Core.Models.File>>> GetAll();
        Task<Result<Core.Models.File>> Get(Guid id);
        Task<Guid> Rename(Guid id, string name);
        Task<Guid> Move(Guid id, string path, Guid folderId);
        Task<Guid> Size(Guid id, long size);
    }
}