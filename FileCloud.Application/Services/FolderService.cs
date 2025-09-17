using FileCloud.Core.Abstractions;
using FileCloud.Core.Models;
using FileCloud.Core;
using Microsoft.Extensions.Logging;

namespace FileCloud.Application.Services
{
    public class FolderService : IFolderService
    {
        IFolderRepository _folderRepository;
        ILogger<FolderService> _logger;
        public FolderService(IFolderRepository repository, ILogger<FolderService> logger)
        {
            this._folderRepository = repository;
            this._logger = logger;
        }
        public async Task<List<Result<Folder>>> GetAllFolders()
        {
            return await _folderRepository.GetAll();
        }
        public async Task<Result<List<Folder>>> GetSubFolder(Guid id)
        {
            var result = await _folderRepository.Get(id);
            if (!result.IsSuccess)
                return Result<List<Folder>>.Fail(result.Error);
            var folder = result.Value;
            return Result<List<Folder>>.Success(folder.SubFolders.ToList()); 
        }
        public async Task<Result<List<Core.Models.File>>> GetFiles(Guid id)
        {
            var result = await _folderRepository.Get(id);
            if (!result.IsSuccess)
                return Result<List<Core.Models.File>>.Fail(result.Error);
            var folder = result.Value;
            return Result<List<Core.Models.File>>.Success(folder.Files.ToList());
        }
        public async Task<Result<Folder>> GetFolder(Guid id)
        {
            return await _folderRepository.Get(id);
        }
        public async Task<Result<Guid>> CreateFolder(string name, Guid? parentId)
        {
            var folderResult = Folder.Create(Guid.NewGuid(), name, parentId);
            if (!folderResult.IsSuccess)
                return Result<Guid>.Fail(folderResult.Error);

            var result = await _folderRepository.Create(folderResult.Value);
            return result;
        }
        public async Task<Result<Guid>> RenameFolder(Guid id, string name)
        {
            var result = await _folderRepository.Rename(id, name);
            return result;
        }
        public async Task<Result<Guid>> MoveFolder(Guid id, Guid? parentId)
        {
            var result = await _folderRepository.Move(id, parentId);
            return result;
        }
        public async Task<Result<Folder>> DeleteFolder(Guid id)
        {
            try
            {
                var result = await _folderRepository.Delete(id);
                return result;
            }
            catch(Exception ex)
            {
                return Result<Folder>.Fail(ex.Message);
            }
        }
    }
}
