using FileCloud.Core.Abstractions;
using FileCloud.Core.Models;
using FileCloud.Core;
using Microsoft.Extensions.Logging;

namespace FileCloud.Application.Services
{
    public class FolderService : IFolderService
    {
        IFolderRepositories _folderRepository;
        ILogger<FolderService> _logger;
        public FolderService(IFolderRepositories repository, ILogger<FolderService> logger)
        {
            this._folderRepository = repository;
            this._logger = logger;
        }
        public async Task<List<Result<Folder>>> GetAllFolders()
        {
            return await _folderRepository.GetAll();
        }
        public async Task<Result<List<Folder>>> GetChildFolder(Guid id)
        {
            var result = await _folderRepository.GetChild(id);
            if(result.Count == 0)
                return Result<List<Folder>>.Success(new List<Folder>());

            var errors = result.Where(f => !f.IsSuccess)
                .Select(e => e.Error)
                .ToList();

            foreach (var error in errors)
                _logger.LogInformation(error);

            var SuccessValues = result.Where(f => f.IsSuccess)
                .Select(v => v.Value!)
                .ToList();

            if (SuccessValues.Count == 0)
                return Result<List<Folder>>.Fail("couldn't get any objects");
            else
                return Result<List<Folder>>.Success(SuccessValues);
        }
        public async Task<Result<Folder>> GetFolder(Guid id)
        {
            return await _folderRepository.Get(id);
        }
        public async Task<Result<Guid>> CreateFolder(string name, Guid? parentId)
        {
            var folderResult = Folder.Create(Guid.NewGuid(), name, parentId, new List<Folder>(), new List<Core.Models.File>());
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
        public async Task<Result<Guid>> MoveFolder(Guid id, Folder? parent)
        {
            var result = await _folderRepository.Move(id, parent);
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
