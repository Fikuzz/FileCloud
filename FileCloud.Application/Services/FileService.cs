using FileCloud.Core;
using FileCloud.Core.Abstractions;
using Microsoft.Extensions.Logging;
using Model = FileCloud.Core.Models;

namespace FileCloud.Application.Services
{
    public class FileService : IFilesService
    {
        private readonly IFilesRepositories _filesRepositories;
        private readonly ILogger<FileService> _logger;
        public FileService(IFilesRepositories filesRepositories, ILogger<FileService> logger)
        {
            _filesRepositories = filesRepositories;
            _logger = logger;
        }

        public async Task<List<Result<Model.File>>> GetAllFiles()
        {
            var resultList = await _filesRepositories.GetAll();

            return resultList;
        }

        public async Task<Result<Model.File>> GetFileById(Guid id)
        {
            return await _filesRepositories.Get(id);
        }

        public async Task<Result<string>> UploadFile(Model.File file)
        {
            try
            {
                var result = await _filesRepositories.Create(file);

                return Result<string>.Success(file.Name);
            }
            catch (Exception ex)
            {
                return Result<string>.Fail(ex.Message);
            }
        }

        public async Task<Result<Guid>> RenameFile(Guid id, string name)
        {
            var result = await _filesRepositories.Rename(id, name);
            return Result<Guid>.Success(result);
        }
        public async Task<Result<Guid>> MoveFile(Guid id, string? newPath, Guid folderId)
        {
            var result = await _filesRepositories.Move(id, newPath, folderId);
            return Result<Guid>.Success(result);
        }

        public async Task<Result<string>> DeleteFile(Guid id)
        {
            var uploadedFile = await _filesRepositories.Get(id);
            if (!uploadedFile.IsSuccess)
                return Result<string>.Fail(uploadedFile.Error);
            var file = uploadedFile.Value;

            try
            {
                var deleteId = await _filesRepositories.Delete(id);
                return Result<string>.Success(file.Name);
            }
            catch (Exception ex)
            {
                return Result<string>.Fail(ex.Message);
            }
        }

        public async Task<Result<List<Model.File>>> GetChildFiles(Guid id)
        {
            var result = await _filesRepositories.GetChild(id);
            if (result.Count == 0)
                return Result<List<Model.File>>.Success(new List<Model.File>());

            var errors = result.Where(f => !f.IsSuccess)
                .Select(e => e.Error)
                .ToList();

            foreach (var error in errors)
                _logger.LogInformation(error);

            var SuccessValues = result.Where(f => f.IsSuccess)
                .Select(v => v.Value)
                .ToList();

            if (SuccessValues.Count == 0)
                return Result<List<Model.File>>.Fail("couldn't get any objects");
            else
                return Result<List<Model.File>>.Success(SuccessValues);
        }
    }
}
