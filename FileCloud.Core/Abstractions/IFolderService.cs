using FileCloud.Core.Models;

namespace FileCloud.Core.Abstractions
{
    public interface IFolderService
    {
        public Task<List<Result<Folder>>> GetAllFolders();
        public Task<Result<List<Folder>>> GetSubFolder(Guid id);
        public Task<Result<List<Core.Models.File>>> GetFiles(Guid id);
        public Task<Result<Folder>> GetFolder(Guid id);
        public Task<Result<Guid>> CreateFolder(string name, Guid? parentId);
        public Task<Result<Guid>> RenameFolder(Guid id, string name);
        public Task<Result<Guid>> MoveFolder(Guid id, Guid? parentId);
        public Task<Result<Folder>> DeleteFolder(Guid id);
    }
}
