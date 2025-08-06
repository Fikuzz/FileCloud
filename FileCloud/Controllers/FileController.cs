using FileCloud.Application.Services;
using FileCloud.Contracts;
using FileCloud.Core.Abstractions;
using FileCloud.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Net.Http;
using Model = FileCloud.Core.Models;

namespace FileCloud.Controllers
{
    [ApiController]
    [Route("api/file")]
    public class FileController : ControllerBase
    {
        private readonly IFilesService _filesService;
        private readonly PreviewService _previewService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FileController> _logger;
        public FileController(IFilesService filesService, IWebHostEnvironment webHostEnvironment, ILogger<FileController> logger, PreviewService previewService)
        {
            _previewService = previewService;
            _filesService = filesService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<FileResponse>>> GetFile()
        {
            var files = await _filesService.GetAllFiles();

            var response = files.Select(f => new FileResponse(f.id, f.Name, f.Path));

            return Ok(response);
        }

        [HttpGet("{id:Guid}")]
        public async Task<ActionResult<FileResponse>> GetFileById(Guid id)
        {
            var file = await _filesService.GetFileById(id);

            var response = new FileResponse(id, file.Name, file.Path);

            return Ok(response);
        }

        [HttpPost("stream-upload")]
        [RequestSizeLimit(long.MaxValue)]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UploadStream([FromQuery] string? path = null)
        {
            if (!Request.HasFormContentType)
                return BadRequest("Некорректный Content-Type");

            var files = Request.Form.Files;

            if (files == null || files.Count == 0)
                return BadRequest("Файлы не были загружены");

            var uploadedFiles = new List<string>();
            var relativePath = path?.Trim('/') ?? string.Empty;
            var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);
            Directory.CreateDirectory(folderPath);

            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file.FileName);
                string filePath = Path.Combine(folderPath, fileName);

                if (System.IO.File.Exists(filePath))
                    continue; // Пропускаем существующий файл

                await using var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
                await file.CopyToAsync(stream);
                await stream.FlushAsync();
                stream.Close();

                _logger.LogInformation("Загружен файл: {File}", filePath);

                Guid fileGuid = await _filesService.UploadFile(Core.Models.File.Create(Guid.NewGuid(), fileName, relativePath).file);
                await _previewService.GeneratePreviewAsync(filePath, fileGuid);
                uploadedFiles.Add(fileName);
            }

            return Ok(new { Uploaded = uploadedFiles });
        }

        [HttpPost("delete")]
        public async Task<ActionResult<List<Guid>>> DeleteFiles([FromBody] List<Guid> ids)
        {
            var deletedFileIds = new List<Guid>();

            foreach (var id in ids)
            {
                var uploadedFile = await _filesService.GetFileById(id);

                if (uploadedFile == null)
                    continue;

                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, uploadedFile.Path, uploadedFile.Name);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                _previewService.DeletePreview(id);
                var deletedId = await _filesService.DeleteFile(id);
                deletedFileIds.Add(deletedId);
            }

            return Ok(deletedFileIds);
        }

        [HttpGet("preview/{id:guid}")]
        public async Task<IActionResult> GetPreview(Guid id)
        {
            var previewPath = _previewService.GetPreviewPath(id);

            if (!System.IO.File.Exists(previewPath))
                return NotFound();

            var bytes = await System.IO.File.ReadAllBytesAsync(previewPath);

            return File(bytes, "image/jpeg");
        }
    }
}
