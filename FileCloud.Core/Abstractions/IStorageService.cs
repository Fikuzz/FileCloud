using FileCloud.Core.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Core.Abstractions
{
    public interface IStorageService
    {
        Task<Result<Core.Models.File>> LoadFileAsStream(Stream fileStream, Core.Models.File fileDTO);
        Task<Result<string>> CreateNewFolder(string name, Guid? parentId);
        Task<Result<Folder>> DeleteFolderCascadeAsync(Guid folderId);
        Result<string> DeleteFolderByPath(string path);
        Task<Result<Core.Models.File>> DeleteFileAsync(Guid fileId);

        Task<Result<string>> MoveFolder(Guid folderId, Guid? newParentId);
        Task<Result<string>> MoveFile(Guid fileId, Guid? newFolderId);

        Task<Result<string>> RenameFolder(Guid folderId, string newName);
        Task<Result<string>> RenameFile(Guid fileId, string newName);

        Task<Result<byte[]>> GetPreview(Guid id);
        Task<Result<byte[]>> GetFileBytes(Guid id);

        Task<Result<string>> BuildFullPathForFileAsync(Guid fileId);
        Task<Result<string>> BuildFullPathForFolderAsync(Guid? folderId);
    }
}
