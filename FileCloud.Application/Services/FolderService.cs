using FileCloud.Core;
using FileCloud.Core.Abstractions;
using FileCloud.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FileCloud.Application.Services
{
    public class FolderService : IFolderService
    {
        IFolderRepository _folderRepository;
        ILogger<FolderService> _logger;
        IUserContext _userContext;
        public FolderService(IFolderRepository repository, ILogger<FolderService> logger, IUserContext userContext)
        {
            this._folderRepository = repository;
            this._logger = logger;
            _userContext = userContext;
        }
        public async Task<List<Result<Folder>>> GetAllFolders()
        {
            if (!_userContext.IsAuthenticated || _userContext.UserId == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            return await _folderRepository.GetAll(_userContext.UserId.Value);
        }
        public async Task<Result<List<Folder>>> GetSubFolder(Guid id)
        {
            if (!_userContext.IsAuthenticated || _userContext.UserId == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var result = await _folderRepository.Get(id, _userContext.UserId.Value);
            if (!result.IsSuccess)
                return Result<List<Folder>>.Fail(result.Error);
            var folder = result.Value;
            return Result<List<Folder>>.Success(folder.SubFolders.ToList()); 
        }
        public async Task<Result<List<Core.Models.File>>> GetFiles(Guid id)
        {
            if (!_userContext.IsAuthenticated || _userContext.UserId == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var result = await _folderRepository.Get(id, _userContext.UserId.Value);
            if (!result.IsSuccess)
                return Result<List<Core.Models.File>>.Fail(result.Error);
            var folder = result.Value;
            return Result<List<Core.Models.File>>.Success(folder.Files.ToList());
        }
        public async Task<Result<Folder>> GetFolder(Guid id)
        {
            if (!_userContext.IsAuthenticated || _userContext.UserId == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            return await _folderRepository.Get(id, _userContext.UserId.Value);
        }
        public async Task<Result<Folder>> CreateFolder(string name, Guid parentId)
        {
            if (!_userContext.IsAuthenticated || _userContext.UserId == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var folderResult = Folder.Create(Guid.NewGuid(), name, parentId, _userContext.UserId.Value);
            if (!folderResult.IsSuccess)
                return Result<Folder>.Fail(folderResult.Error);

            var result = await _folderRepository.Create(folderResult.Value);
            return result;
        }
        public async Task<Result<Folder>> CreateRootFolder(string name, Guid userId)
        {
            var folderResult = Folder.Create(Guid.NewGuid(), name, null, userId);
            if (!folderResult.IsSuccess)
                return Result<Folder>.Fail(folderResult.Error);

            var folder = folderResult.Value;
            folder.IsRoot = true;
            var result = await _folderRepository.Create(folder);
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
