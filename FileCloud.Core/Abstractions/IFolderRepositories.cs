using FileCloud.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Core.Abstractions
{
    public interface IFolderRepositories
    {
        Task<Guid> Create(Folder folder);
        Task<Folder> Delete(Guid id);
        Task<List<Result<Folder>>> GetAll();
        Task<List<Result<Folder>>> GetChild(Guid id);
        Task<Result<Folder>> Get(Guid id);
        Task<Guid> Rename(Guid id, string name);
        Task<Guid> Move(Guid id, Folder? parent);
    }
}
