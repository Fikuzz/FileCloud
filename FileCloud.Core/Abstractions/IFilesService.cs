using FileCloud.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace FileCloud.Core.Abstractions
{
    public interface IFilesService
    {
        Task<List<Result<Core.Models.File>>> GetAllFiles();
        Task<Result<List<Core.Models.File>>> GetChildFiles(Guid id);
        Task<Result<string>> UploadFile(Core.Models.File file);
        Task<Result<Guid>> RenameFile(Guid id, string name);
        Task<Result<Guid>> MoveFile(Guid id, string newPath, Guid parentId);
        Task<Result<string>> DeleteFile(Guid id);
        Task<Result<Core.Models.File>> GetFileById(Guid id);
    }
}