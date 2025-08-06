using FileCloud.Core.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Model = FileCloud.Core.Models;

namespace FileCloud.Application.Services
{
    public class FileService : IFilesService
    {
        private readonly IFilesRepositories _filesRepositories;
        public FileService(IFilesRepositories filesRepositories)
        {
            _filesRepositories = filesRepositories;
        }

        public async Task<List<Model.File>> GetAllFiles()
        {
            return await _filesRepositories.Get();
        }

        public async Task<Model.File> GetFileById(Guid id)
        {
            return await _filesRepositories.GetById(id);
        }

        public async Task<Guid> UploadFile(Model.File file)
        {
            return await _filesRepositories.Create(file);
        }

        public async Task<Guid> UpdateFile(Guid id, string name, string path)
        {
            return await _filesRepositories.Update(id, name, path);
        }

        public async Task<Guid> DeleteFile(Guid id)
        {
            return await _filesRepositories.Delete(id);
        }
    }
}
