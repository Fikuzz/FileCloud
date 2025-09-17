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
    public class FolderRepository : IFolderRepository
    {
        private readonly FileCloudDbContext _context;

        public FolderRepository(FileCloudDbContext context)
        {
            _context = context;
        }
        public async Task<Result<Guid>> Create(Folder folder)
        {
            var folderEntity = new FolderEntity()
            {
                Name = folder.Name,
                ParentId = folder.ParentId
            };
            await _context.Folders.AddAsync(folderEntity);
            await _context.SaveChangesAsync();
            return Result<Guid>.Success(folderEntity.Id);
        }

        public async Task<Result<Folder>> Delete(Guid id)
        {
            var folder = await _context.Folders
                .Where(f => f.Id == id)
                .Select(s => FolderMapper.ToModel(s))
                .FirstAsync();

            if (!folder.IsSuccess)
                return Result<Folder>.Fail($"Folder with id:{id} not found");

            await _context.Folders
                .Where(f => f.Id == id)
                .ExecuteDeleteAsync();

            return Result<Folder>.Success(folder.Value);
        }

        public async Task<Result<Folder>> Get(Guid id)
        {
            try
            {
                var folderEntity = await _context.Folders
                    .Include(f => f.Files)       // Подгружаем файлы
                    .Include(f => f.SubFolders)  // Подгружаем подпапки
                    .FirstOrDefaultAsync(f => f.Id == id);
                if (folderEntity == null)
                    return Result<Folder>.Fail($"Folder with id:{id} not found");
                var result = FolderMapper.ToModel(folderEntity);

                return result;
            }
            catch(Exception ex)
            {
                return Result<Folder>.Fail(ex.Message);
            }
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

        public async Task<Result<Guid>> Move(Guid id, Guid? parentId)
        {
            await _context.Folders
                .Where(f => f.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(f => f.ParentId, _ => parentId));
            return Result<Guid>.Success(id);
        }

        public async Task<Result<Guid>> Rename(Guid id, string name)
        {
            await _context.Folders
                .Where(f => f.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(f => f.Name, _ => name));
            return Result<Guid>.Success(id);
        }
    }
}
