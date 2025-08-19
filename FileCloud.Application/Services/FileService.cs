using FileCloud.Core;
using FileCloud.Core.Abstractions;
using FileCloud.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using Model = FileCloud.Core.Models;

namespace FileCloud.Application.Services
{
    public class FileService : IFilesService
    {
        private readonly IFilesRepositories _filesRepositories;
        private readonly IFolderService _folderService;
        private readonly ILogger<FileService> _logger;
        private readonly PreviewService _previewService;
        private readonly string _basePath;
        public FileService(IFilesRepositories filesRepositories, IFolderService folderService,  string baseFilePath, PreviewService previewService, ILogger<FileService> logger)
        {
            _filesRepositories = filesRepositories;
            _folderService = folderService;
            _basePath = baseFilePath;
            _previewService = previewService;
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

        public async Task<Result<string>> UploadFile(Model.File file, IFormFile fileStream)
        {
            var relativePath = file.Path?.Trim('/') ?? string.Empty;
            var folderPath = Path.Combine(_basePath, relativePath);
            if(!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, file.Name);

            if (System.IO.File.Exists(filePath))
            {
                return Result<string>.Success(file.Name);
            }
            try
            {
                await using var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
                await fileStream.CopyToAsync(stream);
                await stream.FlushAsync();
            }
            catch (Exception ex)
            {
                return Result<string>.Fail(ex.Message);
            }

            var result = await _filesRepositories.Create(file);

            await _previewService.GeneratePreviewAsync(filePath, result);

            _logger.LogInformation("Загружен файл: {File}", filePath);

            return Result<string>.Success(file.Name);
        }

        public async Task<Result<Guid>> RenameFile(Guid id, string name)
        {
            var result = await _filesRepositories.Rename(id, name);
            return Result<Guid>.Success(result);
        }
        public async Task<Result<Guid>> MoveFile(Guid id, Guid folderId)
        {
            var file = await _filesRepositories.Get(id);
            if (!file.IsSuccess)
                return Result<Guid>.Fail("Не удалось получить папку");

            var fullPath = await _folderService.BuildFullPathAsync(file.Value);
            var result = await _filesRepositories.Move(id, fullPath, folderId);
            return Result<Guid>.Success(result);
        }

        public async Task<Result<string>> DeleteFile(Guid id)
        {
            var deleteId = await _filesRepositories.Delete(id);

            var uploadedFile = await _filesRepositories.Get(id);
            if (!uploadedFile.IsSuccess)
                return Result<string>.Fail(uploadedFile.Error);

            var file = uploadedFile.Value;

            if (System.IO.File.Exists(file.Path))
            {
                System.IO.File.Delete(file.Path);
            }
            _previewService.DeletePreview(id);

            return Result<string>.Success(file.Name);
        }

        public async Task<Result<byte[]>> GetPreview(Guid id)
        {
            var previewPath = _previewService.GetPreviewPath(id);

            if (!System.IO.File.Exists(previewPath))
            {
                var fileResult = await _filesRepositories.Get(id);
                if (!fileResult.IsSuccess)
                    return Result<byte[]>.Fail(fileResult.Error);
                var file = fileResult.Value;
                await _previewService.GeneratePreviewAsync(file.Path, id);
                previewPath = _previewService.GetPreviewPath(id);
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(previewPath);

            return Result<byte[]>.Success(bytes);
        }

        public async Task<Result<byte[]>> GetFileBytes(Guid id)
        {
            var fileResult = await _filesRepositories.Get(id);
            if (!fileResult.IsSuccess)
            {
                _logger.LogWarning(fileResult.Error);
                return Result<byte[]>.Fail(fileResult.Error);
            }
            var file = fileResult.Value;
            if (!System.IO.File.Exists(file.Path))
            {
                var error = $"File missing on disk: {file.Path}";
                _logger.LogWarning(error);
                return Result<byte[]>.Fail(error);
            }

            _logger.LogInformation($"Отправка файла клиенту: {file.Path}");

            var bytes = await System.IO.File.ReadAllBytesAsync(file.Path);
            return Result<byte[]>.Success(bytes);
        }
    }
}
