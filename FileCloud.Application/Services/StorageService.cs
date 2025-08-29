using FileCloud.Core;
using FileCloud.Core.Abstractions;
using FileCloud.Core.Models;
using FileCloud.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileCloud.Application.Services
{
    public class StorageService : IStorageService
    {
        private readonly IFilesService _filesService;
        private readonly IFolderService _folderService;
        private readonly string _basePath;
        private readonly PreviewService _previewService;
        private readonly ILogger<StorageService> _logger;

        public StorageService(IFilesService filesService, IFolderService folderService, IOptions<StorageOptions> options, PreviewService previewService, ILogger<StorageService> logger)
        {
            _filesService = filesService;
            _folderService = folderService;
            _basePath = options.Value.BasePath;
            _previewService = previewService;
            _logger = logger;
        }
        
        public async Task<Result<Core.Models.File>> LoadFileAsStream(Stream fileStream, Core.Models.File fileDTO)
        {
            var folderPathResult = await BuildFullPathForFolderAsync(fileDTO.FolderId);

            if (!folderPathResult.IsSuccess)
                return Result<Core.Models.File>.Fail(folderPathResult.Error);

            var folderPath = Path.Combine(_basePath, folderPathResult.Value);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, fileDTO.Name);

            if (System.IO.File.Exists(filePath))
            {
                return Result<Core.Models.File>.Fail("The file already exist");
            }
            try
            {
                await using var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
                await fileStream.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                return Result<Core.Models.File>.Fail(ex.Message);
            }

            await _previewService.GeneratePreviewAsync(filePath, fileDTO.Id);

            _logger.LogInformation("Загружен файл: {File}", filePath);

            fileDTO.Path = folderPath;
            return Result<Core.Models.File>.Success(fileDTO);
        }
        public async Task<Result<string>> CreateNewFolder(string name, Guid? parentId)
        {
            Result<string> pathResult = Result<string>.Success(string.Empty);
            if(parentId.HasValue)
                pathResult = await BuildFullPathForFolderAsync(parentId.Value);

            if (!pathResult.IsSuccess)
                return Result<string>.Fail(pathResult.Error);

            var fullPath = Path.Combine(_basePath, pathResult.Value, name);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return Result<string>.Success(fullPath);
        }
    
        public async Task<Result<Folder>> DeleteFolderCascadeAsync(Guid folderId)
        {
            var folderResult = await _folderService.GetFolder(folderId);
            if (!folderResult.IsSuccess)
                return Result<Folder>.Fail(folderResult.Error);

            // удалить файлы
            foreach (var file in folderResult.Value.Files)
            {
                await DeleteFileAsync(file.Id);
                await _filesService.DeleteFile(file.Id);
            }

            // удалить подпапки (рекурсивно)
            foreach (var subFolder in folderResult.Value.SubFolders)
            {
                await DeleteFolderCascadeAsync(subFolder.Id);
            }

            // удалить саму папку
            var folderPath = await BuildFullPathForFolderAsync(folderId);
            if (!folderPath.IsSuccess)
                return Result<Folder>.Fail(folderPath.Error);

            var fullPath = Path.Combine(_basePath, folderPath.Value);
            if (Directory.Exists(fullPath))
                Directory.Delete(fullPath);

            await _folderService.DeleteFolder(folderId);

            return Result<Folder>.Success(folderResult.Value);
        }
        public Result<string> DeleteFolderByPath(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path);
                }
            }
            catch (Exception ex)
            {
                return Result<string>.Fail(ex.Message);
            }
            return Result<string>.Success(path);
        }
        public async Task<Result<Core.Models.File>> DeleteFileAsync(Guid fileId)
        {
            var file = await _filesService.GetFileById(fileId);
            if (!file.IsSuccess)
                return Result<Core.Models.File>.Fail(file.Error);

            var path = await BuildFullPathForFileAsync(fileId);
            if (!path.IsSuccess)
                return Result<Core.Models.File>.Fail(path.Error);

            var fullpath = Path.Combine(_basePath, path.Value);

            if (System.IO.File.Exists(fullpath))
                System.IO.File.Delete(fullpath);

            return Result<Core.Models.File>.Success(file.Value);
        }

        public async Task<Result<string>> MoveFolder(Guid folderId, Guid? newParentId)
        {
            var folderResult = await _folderService.GetFolder(folderId);
            if (!folderResult.IsSuccess)
                return Result<string>.Fail(folderResult.Error);

            var oldPath = await BuildFullPathForFolderAsync(folderId);
            var newPath = await BuildFullPathForFolderAsync(newParentId);

            if (!oldPath.IsSuccess)
                return Result<string>.Fail(oldPath.Error);
            if (!newPath.IsSuccess)
                return Result<string>.Fail(newPath.Error);

            var destinationPath = Path.Combine(_basePath, newPath.Value, folderResult.Value.Name);
            if (Directory.Exists(destinationPath))
                return Result<string>.Fail("Target folder already exists");

            try
            {
                Directory.Move(
                    Path.Combine(_basePath, oldPath.Value),
                    destinationPath);
            }
            catch(Exception ex)
            {
                return Result<string>.Fail(ex.Message);
            }
            return Result<string>.Success(newPath.Value);
        }
        public async Task<Result<string>> MoveFile(Guid fileId, Guid? newFolderId)
        {
            var fileResult = await _filesService.GetFileById(fileId);
            if (!fileResult.IsSuccess)
                return Result<string>.Fail(fileResult.Error);

            var oldPath = await BuildFullPathForFileAsync(fileId);
            var newPath = await BuildFullPathForFolderAsync(newFolderId);

            if (!oldPath.IsSuccess)
                return Result<string>.Fail(oldPath.Error);
            if (!newPath.IsSuccess)
                return Result<string>.Fail(newPath.Error);

            var destinationPath = Path.Combine(_basePath, newPath.Value, fileResult.Value.Name);
            if(System.IO.File.Exists(destinationPath))
                return Result<string>.Fail("Target file already exists");
            try
            {
                System.IO.File.Move(
                    Path.Combine(_basePath, oldPath.Value),
                    destinationPath);
            }
            catch (Exception ex)
            {
                return Result<string>.Fail(ex.Message);
            }
            return Result<string>.Success(newPath.Value);
        }

        public async Task<Result<string>> RenameFolder(Guid folderId, string newName)
        {
            var oldPathResult = await BuildFullPathForFolderAsync(folderId);
            if (!oldPathResult.IsSuccess)
                return Result<string>.Fail(oldPathResult.Error);

            var oldPath = Path.Combine(_basePath, oldPathResult.Value);

            // Родительская папка
            var parent = Directory.GetParent(oldPath)?.FullName;
            if (parent == null)
                return Result<string>.Fail("Parent folder not found");

            var newPath = Path.Combine(parent, newName);

            if (Directory.Exists(newPath))
                return Result<string>.Fail("Folder with the same name already exists");

            try
            {
                Directory.Move(oldPath, newPath);
            }
            catch (Exception ex)
            {
                return Result<string>.Fail(ex.Message);
            }

            return Result<string>.Success(newPath);
        }
        public async Task<Result<string>> RenameFile(Guid fileId, string newName)
        {
            var oldPathResult = await BuildFullPathForFileAsync(fileId);
            if (!oldPathResult.IsSuccess)
                return Result<string>.Fail(oldPathResult.Error);

            var oldPath = Path.Combine(_basePath, oldPathResult.Value);

            var parent = Path.GetDirectoryName(oldPath);
            if (parent == null)
                return Result<string>.Fail("Parent folder not found");

            var newPath = Path.Combine(parent, newName);

            if (System.IO.File.Exists(newPath))
                return Result<string>.Fail("File with the same name already exists");

            try
            {
                System.IO.File.Move(oldPath, newPath);
            }
            catch (Exception ex)
            {
                return Result<string>.Fail(ex.Message);
            }

            return Result<string>.Success(newPath);
        }

        public async Task<Result<byte[]>> GetPreview(Guid id)
        {
            var previewPath = _previewService.GetPreviewPath(id);

            if (!System.IO.File.Exists(previewPath))
            {
                var fileResult = await _filesService.GetFileById(id);
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
            var fileResult = await _filesService.GetFileById(id);
            if (!fileResult.IsSuccess)
            {
                _logger.LogWarning(fileResult.Error);
                return Result<byte[]>.Fail(fileResult.Error);
            }
            var filePath = Path.Combine(fileResult.Value.Path, fileResult.Value.Name);
            if (!System.IO.File.Exists(filePath))
            {
                var error = $"File missing on disk: {filePath}";
                _logger.LogWarning(error);
                return Result<byte[]>.Fail(error);
            }

            _logger.LogInformation($"Отправка файла клиенту: {filePath}");

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return Result<byte[]>.Success(bytes);
        }

        public async Task<Result<string>> BuildFullPathForFileAsync(Guid fileId)
        {
            var fileResult = await _filesService.GetFileById(fileId);
            if (!fileResult.IsSuccess)
                return Result<string>.Fail("File not found");

            return await BuildFullPathAsync(fileResult.Value.FolderId, fileResult.Value.Name);
        }
        public Task<Result<string>> BuildFullPathForFolderAsync(Guid? folderId) =>
            BuildFullPathAsync(folderId);

        public async Task<Result<string>> BuildFullPathAsync(Guid? folderId, string? fileName = null)
        {
            if (folderId == null)
                return Result<string>.Success(string.Empty);

            var parts = new List<string>();

            var currentFolderResult = await _folderService.GetFolder(folderId.Value);
            if (!currentFolderResult.IsSuccess)
                return Result<string>.Fail("Folder not found");

            var current = currentFolderResult.Value;
            while (current != null)
            {
                parts.Add(current.Name);

                if (current.ParentId == null) break;

                var parentResult = await _folderService.GetFolder(current.ParentId.Value);
                if (!parentResult.IsSuccess) break;

                current = parentResult.Value;
            }

            parts.Reverse();
            if (fileName != null)
                parts.Add(fileName);

            return Result<string>.Success(Path.Combine(parts.ToArray()));
        }
    }
}
