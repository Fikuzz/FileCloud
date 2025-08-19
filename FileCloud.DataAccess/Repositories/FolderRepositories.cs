using FileCloud.Core.Abstractions;
using FileCloud.Core.Models;
using FileCloud.DataAccess.Entities;


using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileCloud.DataAccess.Mappers;
using FileCloud.Core;

namespace FileCloud.DataAccess.Repositories
{
    public class FolderRepositories : IFolderRepositories
    {
        private readonly FileCloudDbContext _context;

        public FolderRepositories(FileCloudDbContext context)
        {
            _context = context;
        }
        public async Task<Guid> Create(Folder folder)
        {
            var folderEntity = new FolderEntity()
            {
                Name = folder.Name,
                ParentId = folder.ParentId
            };
            await _context.Folders.AddAsync(folderEntity);
            await _context.SaveChangesAsync();
            return folderEntity.Id;
        }

        public async Task<Folder> Delete(Guid id)
        {
            var folder = await _context.Folders
                .Where(f => f.Id == id)
                .Select(s => FolderMapper.ToModel(s))
                .FirstAsync();

            if (!folder.IsSuccess)
                return null;

            await _context.Folders
                .Where(f => f.Id == id)
                .ExecuteDeleteAsync();

            return folder.Value;
        }

        public async Task<Result<Folder>> Get(Guid id)
        {
            var folderEntity = await _context.Folders
                .Where(f => f.Id == id)
                .FirstAsync();

            folderEntity.SubFolders = await _context.Folders
                .Where(f => f.ParentId == id)
                .ToListAsync();

            folderEntity.Files = await _context.Files
                .Where(f => f.FolderId == id)
                .ToListAsync();

            var result = FolderMapper.ToModel(folderEntity);

            return result;
        }

        public async Task<List<Result<Folder>>> GetAll()
        {
            var folderEntities = await _context.Folders
                .AsNoTracking()
                .ToListAsync();

            var result = folderEntities
                .Select(f => FolderMapper.ToModel(f))
                .ToList();

            return result;
        }

        public async Task<List<Result<Folder>>> GetChild(Guid id)
        {
            var fileEntities = await _context.Folders
                .Where(f => f.ParentId == id)
                .AsNoTracking()
                .ToListAsync();

            var result = fileEntities
                .Select(f => FolderMapper.ToModel(f))
                .ToList();

            return result;
        }

        public async Task<Guid> Move(Guid id, Folder? parent)
        {
            var newParentId = parent?.Id;

            await _context.Folders
                .Where(f => f.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(f => f.ParentId, _ => newParentId));
            return id;
        }

        public async Task<Guid> Rename(Guid id, string name)
        {
            await _context.Folders
                .Where(f => f.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(f => f.Name, _ => name));
            return id;
        }
    }
}
