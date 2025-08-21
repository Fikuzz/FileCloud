using FileCloud.Core;
using FileCloud.Core.Abstractions;
using FileCloud.Core.Models;
using FileCloud.DataAccess.Entities;
using FileCloud.DataAccess.Mappers;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Model = FileCloud.Core.Models;

namespace FileCloud.DataAccess.Repositories
{
    public class FileRepositories : IFilesRepositories
    {
        private readonly FileCloudDbContext _context;

        public FileRepositories(FileCloudDbContext context)
        {
            _context = context;
        }

        public async Task<List<Result<Model.File>>> GetAll()
        {
            var fileEntities = await _context.Files
                .AsNoTracking()
                .ToListAsync();

            var result = fileEntities
                .Select(f => FileMapper.ToModel(f))
                .ToList();

            return result;
        }

        public async Task<Result<Model.File>> Get(Guid id)
        {
            var fileEntity = await _context.Files
                .Where(f => f.Id == id)
                .FirstAsync();

            var result = FileMapper.ToModel(fileEntity);

            return result;
        }

        public async Task<Guid> Create(Model.File file)
        {
            var fileEntity = new FileEntity
            {
                Id = file.Id,
                Name = file.Name,
                Path = file.Path,
                Size = file.Size,
                FolderId = file.FolderId,
            };

            await _context.Files.AddAsync(fileEntity);
            await _context.SaveChangesAsync();

            return fileEntity.Id;
        }

        public async Task<Guid> Delete(Guid id)
        {
            await _context.Files
                .Where(f => f.Id == id)
                .ExecuteDeleteAsync();

            return id;
        }

        public async Task<Guid> Rename(Guid id, string name)
        {
            await _context.Files
                .Where(f => f.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(f => f.Name, f => name));
            return id;
        }

        public async Task<Guid> Move(Guid id, string path, Guid folderId)
        {
            await _context.Files
                .Where (f => f.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(f => f.FolderId, f => folderId)
                    .SetProperty(f => f.Path, f => path));
            return id;
        }

        public async Task<Guid> Size(Guid id, long size)
        {
            await _context.Files
                .Where(f => f.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(f => f.Size, f => size));
            return id;
        }

        public async Task<List<Result<Model.File>>> GetChild(Guid id)
        {
            var fileEntities = await _context.Files
                .Where(f => f.FolderId == id)
                .AsNoTracking()
                .ToListAsync();

            var result = fileEntities
                .Select(f => FileMapper.ToModel(f))
                .ToList();

            return result;
        }
    }
}
