using FileCloud.Contracts;
using FileCloud.Core.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Model = FileCloud.Core.Models;

namespace FileCloud.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFilesService _filesService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public FileController(IFilesService filesService, IWebHostEnvironment webHostEnvironment)
        {
            _filesService = filesService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<ActionResult<List<FileResponse>>> GetFile()
        {
            var files = await _filesService.GetAllFiles();

            var ressponse = files.Select(f => new FileResponse(f.id, f.Name, f.Path));

            return Ok(ressponse);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> UploadFile(IFormFile uploadedFile, string ?path)
        {
            if(uploadedFile == null)
            {
                return BadRequest("Couldn't get the file");
            }

            string filePath = $"{_webHostEnvironment.WebRootPath}/{path ?? string.Empty}/";
            
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            
            }
            if (System.IO.File.Exists(filePath + uploadedFile.FileName))
            {
                return BadRequest("A file with that name already exists.");
            }
            using (var fileStream = new FileStream(filePath + uploadedFile.FileName, FileMode.CreateNew))
            {
                await uploadedFile.CopyToAsync(fileStream);
            }

            var (file, error) = Model.File.Create(
                Guid.NewGuid(),
                uploadedFile.FileName,
                path ?? String.Empty);

            if (!string.IsNullOrEmpty(error))
            {
                return BadRequest(error);
            }

            await _filesService.UploadFile(file);

            return Ok(file);
        }
        /*
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Guid>> UpdateFile(Guid id, [FromBody] FileRequest request)
        {
            var fileId = await _filesService.UpdateFile(id, data.Name, request.Path);

            return Ok(fileId);
        }
        */

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Guid>> DeleteFile(Guid id)
        {
            var uploadedFile = await _filesService.GetFileWithId(id);

            string filePath = $"{_webHostEnvironment.WebRootPath}/{uploadedFile.Path}/{uploadedFile.Name}";
            
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            var fileId = await _filesService.DeleteFile(id);

            return fileId;
        }
    }
}
