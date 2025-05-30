using System.Reflection;

namespace FileCloud.Core.Abstractions
{
    public interface IFilesRepositories
    {
        Task<Guid> Create(Core.Models.File file);
        Task<Guid> Delete(Guid id);
        Task<List<Core.Models.File>> Get();
        Task<Core.Models.File> GetWithId(Guid id);
        Task<Guid> Update(Guid id, string name, string path);
    }
}