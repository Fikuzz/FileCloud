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
        Task<Guid> Delete(Guid id);
        Task<List<Result<Folder>>> GetAll();
        Task<Result<Folder>> Get(Guid id);
        Task<Guid> Rename(Guid id, string name);
        Task<Guid> Move(Guid id, Folder? parent);
    }
}
