using FileCloud.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Model = FileCloud.Core.Models;
using FileCloud.Core.Abstractions;

namespace FileCloud.DataAccess.Repositories
{
    public class FileRepositories : IFilesRepositories
    {
        private readonly FileCloudDbContext _context;

        public FileRepositories(FileCloudDbContext context)
        {
            _context = context;
        }

        public async Task<List<Model.File>> Get()
        {
            var fileEntities = await _context.Files
                .AsNoTracking()
                .ToListAsync();

            var files = fileEntities
                .Select(f => Model.File.Create(f.id, f.Name, f.Path).file)
                .ToList();

            return files;
        }

        public async Task<Model.File> GetWithId(Guid id)
        {
            var fileEntity = await _context.Files
                .Where(f => f.id == id)
                .FirstAsync();

            var file = Model.File.Create(fileEntity.id, fileEntity.Name, fileEntity.Path).file;

            return file;
        }

        public async Task<Guid> Create(Model.File file)
        {
            var fileEntity = new FileEntitiy
            {
                Name = file.Name,
                Path = file.Path
            };

            await _context.Files.AddAsync(fileEntity);
            await _context.SaveChangesAsync();

            return fileEntity.id;
        }

        public async Task<Guid> Update(Guid id, string name, string path)
        {
            await _context.Files
                .Where(f => f.id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(f => f.Name, f => name)
                    .SetProperty(f => f.Path, f => path));

            return id;
        }

        public async Task<Guid> Delete(Guid id)
        {
            await _context.Files
                .Where(f => f.id == id)
                .ExecuteDeleteAsync();

            return id;
        }
    }
}
