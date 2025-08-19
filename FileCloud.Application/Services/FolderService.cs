using FileCloud.Core.Abstractions;
using FileCloud.Core.Models;
using FileCloud.Core;

namespace FileCloud.Application.Services
{
    public class FolderService : IFolderService
    {
        IFolderRepositories _folderRepository;
        public FolderService(IFolderRepositories repository)
        {
            this._folderRepository = repository;
        }
        public async Task<List<Result<Folder>>> GetAllFolders()
        {
            return await _folderRepository.GetAll();
        }
        public async Task<Result<Folder>> GetFolder(Guid id)
        {
            return await _folderRepository.Get(id);
        }
        public async Task<Result<Guid>> CreateFolder(Folder folder)
        {
            var result = await _folderRepository.Create(folder);
            return Result<Guid>.Success(result);
        }
        public async Task<Result<Guid>> RenameFolder(Guid id, string name)
        {
            var result = await _folderRepository.Rename(id, name);
            return Result<Guid>.Success(result);
        }
        public async Task<Result<Guid>> MoveFolder(Guid id, Folder? parent)
        {
            var result = await _folderRepository.Move(id, parent);
            return Result<Guid>.Success(result);
        }
        public async Task<Result<Guid>> DeleteFolder(Guid id)
        {
            var result = await _folderRepository.Delete(id);
            return Result<Guid>.Success(result);
        }

        public async Task<string> BuildFullPathAsync(FileCloud.Core.Models.File file)
        {
            var parts = new List<string>();
            parts.Add(file.Name);
            var folder = await _folderRepository.Get(file.FolderId);
            if (!folder.IsSuccess)
                return string.Empty;
            var current = folder.Value;

            while (current != null)
            {
                parts.Add(current.Name);

                if (current.ParentId == null)
                    break;

                var result = await _folderRepository.Get(current.ParentId.Value);
                if (result.IsSuccess)
                    current = result.Value;
                else
                    return string.Empty;
            }

            parts.Reverse();
            return Path.Combine(parts.ToArray());
        }
    }
}
