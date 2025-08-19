using FileCloud.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace FileCloud.Core.Abstractions
{
    public interface IFilesService
    {
        Task<List<Result<Core.Models.File>>> GetAllFiles();
        Task<Result<string>> UploadFile(Core.Models.File file, IFormFile fileStream);
        Task<Result<Guid>> RenameFile(Guid id, string name);
        Task<Result<Guid>> MoveFile(Guid id, Guid parentId);
        Task<Result<string>> DeleteFile(Guid id);
        Task<Result<Core.Models.File>> GetFileById(Guid id);
        Task<Result<byte[]>> GetPreview(Guid id);
        Task<Result<byte[]>> GetFileBytes(Guid id);
    }
}