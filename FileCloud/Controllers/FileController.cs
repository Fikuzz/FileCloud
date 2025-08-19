using FileCloud.Contracts;
using FileCloud.Core.Abstractions;
using FileCloud.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SixLabors.ImageSharp;

namespace FileCloud.Controllers
{
    [ApiController]
    [Route("api/file")]
    public class FileController : ControllerBase
    {
        private readonly IFilesService _filesService;
        private readonly IStorageService _storageService;
        private readonly IHubContext<FileHub> _hubContext;
        public FileController(IFilesService filesService, IStorageService storageService, IHubContext<FileHub> hubContext)
        {
            _filesService = filesService;
            _hubContext = hubContext;
            _storageService = storageService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Contracts.FileResult>>> GetFiles()
        {
            var results = await _filesService.GetAllFiles();

            var response = results.Select(r => new Contracts.FileResult
            {
                File = r.IsSuccess ? new FileResponse(r.Value.Id, r.Value.Name, r.Value.Size, r.Value.Path) : null,
                Error = r.IsSuccess ? null : r.Error
            }).ToList();

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
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> UploadStream([FromQuery] Guid folderId, [FromQuery] string? path = null)
        {
            if (!Request.HasFormContentType)
                return BadRequest("Некорректный Content-Type");

            var files = Request.Form.Files;
            if (files == null || files.Count == 0)
                return BadRequest("Файлы не были загружены");

            var uploadedFiles = new List<string>();
            
            foreach (var file in files)
            {
                var fileDTO = Core.Models.File.Create(Guid.NewGuid(), file.FileName, path, file.Length, folderId);
                
                if (!fileDTO.IsSuccess)
                    return BadRequest(fileDTO.Error);

                await using var fileStream = file.OpenReadStream();
                var result = await _storageService.LoadFileAsStream(fileStream, fileDTO.Value);
                if (!result.IsSuccess)
                    return BadRequest(result.Error);

                var dbResult = await _filesService.UploadFile(result.Value);
                if(!dbResult.IsSuccess)
                {
                    await _storageService.DeleteFileAsync(fileDTO.Value.Id);
                    return BadRequest(dbResult.Error);
                }

                await _hubContext.Clients.All.SendAsync("FileLoaded", result.Value.ToString());

                uploadedFiles.Add(file.FileName);
            }

            return Ok(new { Uploaded = uploadedFiles });
        }

        [HttpPost("delete")]
        public async Task<ActionResult<string>> DeleteFile([FromBody] Guid id)
        {
            var deleteResult = await _storageService.DeleteFileAsync(id);
            if(!deleteResult.IsSuccess)
                return BadRequest(deleteResult.Error);

            var deletedFileName = await _filesService.DeleteFile(id);
            if (!deletedFileName.IsSuccess)
                return BadRequest(deletedFileName.Error);

            return Ok(deletedFileName.Value);
        }

        [HttpGet("preview/{id:guid}")]
        public async Task<ActionResult> GetPreview(Guid id)
        {
            var bytes = await _storageService.GetPreview(id);

            if(!bytes.IsSuccess)
                return NotFound(bytes.Error);

            return File(bytes.Value, "image/jpeg");
        }

        [HttpGet("download/{id:guid}")]
        public async Task<ActionResult> DownloadFile(Guid id)
        {
            var bytesResult = await _storageService.GetFileBytes(id);
            if (!bytesResult.IsSuccess)
                return NotFound(bytesResult.Error);

            var file = await _filesService.GetFileById(id);

            if (!file.IsSuccess)
                return BadRequest(file.Error);

            return File(bytesResult.Value, "application/octet-stream", file.Value.Name);
        }

        [HttpPut("{id}/rename")]
        public async Task<ActionResult> RenameFile(Guid id, [FromBody] RenameFileRequest dto)
        {
            var result = await _storageService.RenameFile(id, dto.NewName);
            if(!result.IsSuccess)
                return BadRequest(result.Error);

            var updated = await _filesService.RenameFile(id, dto.NewName);
            if (!updated.IsSuccess)
            {
                var oldNameResult = await _filesService.GetFileById(id);
                var oldName = id.ToString();
                if (oldNameResult.IsSuccess)
                     oldName = oldNameResult.Value.Name;
                await _storageService.RenameFile(id, oldName);
                return BadRequest(updated.Error);
            }
            return Ok(updated.Value);
        }

        [HttpPut("{id}/move")]
        public async Task<ActionResult> MoveFile(Guid id, [FromBody] Guid folderId)
        {
            var oldfolderResult = await _filesService.GetFileById(id);
            if(!oldfolderResult.IsSuccess)
                return NotFound();

            var result = await _storageService.MoveFile(id, folderId);
            if(!result.IsSuccess)
                return BadRequest(result.Error);

            var updated = await _filesService.MoveFile(id, result.Value, folderId);
            if (!updated.IsSuccess)
            {
                _storageService.MoveFile(id, oldfolderResult.Value.FolderId);
                return BadRequest(updated.Error);
            }
            return Ok(updated.Value);
        }
    }
}
