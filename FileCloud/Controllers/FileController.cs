using FileCloud.Application.Services;
using FileCloud.Contracts;
using FileCloud.Core.Abstractions;
using FileCloud.Core.Models;
using FileCloud.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Model = FileCloud.Core.Models;

namespace FileCloud.Controllers
{
    [ApiController]
    [Route("api/file")]
    public class FileController : ControllerBase
    {
        private readonly IFilesService _filesService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<FileHub> _hubContext;
        public FileController(IFilesService filesService, IWebHostEnvironment webHostEnvironment, IHubContext<FileHub> hubContext)
        {
            _filesService = filesService;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<List<FileResponse>>> GetFile()
        {
            var results = await _filesService.GetAllFiles();

            if(!results.All(r => r.IsSuccess))
            {
                var error = results
                    .Where(r => !r.IsSuccess)
                    .First().Error;
                return BadRequest(error);
            }
            var files = results
                .Select(r => r.Value)
                .ToList();
            var response = files.Select(f => new FileResponse(f.Id, f.Name, f.Size, f.Path));

            return Ok(response);
        }

        [HttpGet("{id:Guid}")]
        public async Task<ActionResult<FileResponse>> GetFileById(Guid id)
        {
            var result = await _filesService.GetFileById(id);
            if (!result.IsSuccess) 
                return BadRequest(result.Error);
            var file = result.Value;
            var response = new FileResponse(id, file.Name, file.Size, file.Path);
            return Ok(response);
        }

        [HttpPost("stream-upload")]
        [RequestSizeLimit(long.MaxValue)]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UploadStream([FromQuery] Guid folderId, [FromQuery] string? path = null)
        {
            if (!Request.HasFormContentType)
                return BadRequest("Некорректный Content-Type");

            var files = Request.Form.Files;
            if (files == null || files.Count == 0)
                return BadRequest("Файлы не были загружены");

            var uploadedFiles = new List<string>();
            
            foreach (var file in files)
            {
                var fileDTO = Core.Models.File.Create(Guid.NewGuid(), file.FileName, path, null, folderId);
                
                if (!fileDTO.IsSuccess)
                    return BadRequest(fileDTO.Error);

                var result = await _filesService.UploadFile(fileDTO.Value, file);

                if (!result.IsSuccess)
                    return BadRequest(result.Error);

                await _hubContext.Clients.All.SendAsync("FileLoaded", result.Value.ToString());

                uploadedFiles.Add(file.FileName);
            }

            return Ok(new { Uploaded = uploadedFiles });
        }

        [HttpPost("delete")]
        public async Task<ActionResult<List<string>>> DeleteFiles([FromBody] List<Guid> ids)
        {
            var deletedFileIds = new List<string>();
            foreach (var id in ids)
            {
                var deletedFileName = await _filesService.DeleteFile(id);
                if (!deletedFileName.IsSuccess)
                    return BadRequest(deletedFileName.Error);
                
                deletedFileIds.Add(deletedFileName.Value);
            }

            return Ok(deletedFileIds);
        }

        [HttpGet("preview/{id:guid}")]
        public async Task<IActionResult> GetPreview(Guid id)
        {
            var bytes = await _filesService.GetPreview(id);

            if(!bytes.IsSuccess)
                return NotFound(bytes.Error);

            return File(bytes.Value, "image/jpeg");
        }

        [HttpGet("download/{id:guid}")]
        public async Task<IActionResult> DownloadFile(Guid id)
        {
            var bytesResult = await _filesService.GetFileBytes(id);
            if (!bytesResult.IsSuccess)
                return NotFound(bytesResult.Error);

            var file = await _filesService.GetFileById(id);

            if (!file.IsSuccess)
                return BadRequest(file.Error);

            return File(bytesResult.Value, "application/octet-stream", file.Value.Name);
        }

        [HttpPut("{id}/rename")]
        public async Task<IActionResult> RenameFile(Guid id, [FromBody] RenameFileRequest dto)
        {
            var updated = await _filesService.RenameFile(id, dto.NewName);
            if(!updated.IsSuccess)
                return BadRequest(updated.Error);
            return Ok(updated.Value);
        }

        [HttpPut("{id}/move")]
        public async Task<IActionResult> MoveFile(Guid id, [FromBody] Guid folderId)
        {
            var updated = await _filesService.MoveFile(id, folderId);
            return Ok(updated);
        }
    }
}
