using FileCloud.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Core.Abstractions
{
    public interface IFolderService
    {
        public Task<List<Result<Folder>>> GetAllFolders();
        public Task<Result<List<Folder>>> GetChildFolder(Guid id);
        public Task<Result<Folder>> GetFolder(Guid id);
        public Task<Result<Guid>> CreateFolder(string name, Guid? parentId);
        public Task<Result<Guid>> RenameFolder(Guid id, string name);
        public Task<Result<Guid>> MoveFolder(Guid id, Folder? parent);
        public Task<Result<Folder>> DeleteFolder(Guid id);
    }
}
