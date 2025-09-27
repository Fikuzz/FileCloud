using FileCloud.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Core.Abstractions
{
    public interface IFolderRepository
    {
        Task<Result<Folder>> Create(Folder folder);
        Task<Result<Folder>> Delete(Guid id);
        Task<List<Result<Folder>>> GetAll(Guid userId);
        Task<List<Result<Folder>>> GetChild(Guid id);
        Task<Result<Folder>> Get(Guid id, Guid userId);
        Task<Result<Guid>> Rename(Guid id, string name);
        Task<Result<Guid>> Move(Guid id, Guid? parentId);
        Task<bool> IsOwnerAsync(Guid folderId, Guid userId);
    }
}
