using FileCloud.Core.Abstractions;
using FileCloud.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
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

        public async Task<List<Model.File>> Get()
        {
            var fileEntities = await _context.Files
                .AsNoTracking()
                .ToListAsync();

            var files = fileEntities
                .Select(f => Model.File.Create(f.Id, f.Name, f.Path).file)
                .ToList();

            return files;
        }

        public async Task<Model.File> GetById(Guid id)
        {
            var fileEntity = await _context.Files
                .Where(f => f.Id == id)
                .FirstAsync();

            var file = Model.File.Create(fileEntity.Id, fileEntity.Name, fileEntity.Path).file;

            return file;
        }

        public async Task<Guid> Create(Model.File file)
        {
            var fileEntity = new FileEntity
            {
                Name = file.Name,
                Path = file.Path
            };

            await _context.Files.AddAsync(fileEntity);
            await _context.SaveChangesAsync();

            return fileEntity.Id;
        }

        public async Task<Guid> Update(Guid id, string name, string path)
        {
            await _context.Files
                .Where(f => f.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(f => f.Name, f => name)
                    .SetProperty(f => f.Path, f => path));

            return id;
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

        public async Task<Guid> Move(Guid id, string path)
        {
            await _context.Files
                .Where(f => f.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(f => f.Path, f => path));
            return id;
        }
    }
}
