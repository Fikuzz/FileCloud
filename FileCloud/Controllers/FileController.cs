using FileCloud.Contracts;
using FileCloud.Contracts.Requests.File;
using FileCloud.Contracts.Requests.Folder;
using FileCloud.Contracts.Responses.File;
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
        public async Task<ActionResult<List<ApiResult<FileResponse>>>> GetFiles()
        {
            var results = await _filesService.GetAllFiles();

            var response = results.Select(r => new ApiResult<FileResponse>
            {
                Response = r.IsSuccess ? new FileResponse(r.Value.Id, r.Value.Name, r.Value.Size) : null,
                Error = r.IsSuccess ? null : r.Error
            }).ToList();

            return Ok(response);
        }

        [HttpGet("{id:Guid}")]
        public async Task<ActionResult<ApiResult<FileResponse>>> GetFileById(Guid id)
        {
            var result = await _filesService.GetFileById(id);
            if (!result.IsSuccess) 
                return BadRequest(ApiResult<FileResponse>.Fail(result.Error));

            var file = result.Value;
            var response = new FileResponse(id, file.Name, file.Size);
            return Ok(ApiResult<FileResponse>.Success(response));
        }

        [HttpPost("stream-upload")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<ApiResult<FileResponse>>> UploadFile(
            [FromForm] Guid folderId,
            [FromForm] IFormFile file)
        {
            if (!Request.HasFormContentType)
                return BadRequest(ApiResult<FileResponse>.Fail("Некорректный Content-Type"));

            if (file == null)
                return BadRequest(ApiResult<FileResponse>.Fail("Файл не был загружен"));

            // Создание объекта файла
            var fileDTO = Core.Models.File.Create(Guid.NewGuid(), file.FileName, string.Empty, file.Length, folderId);
            if (!fileDTO.IsSuccess)
                return BadRequest(ApiResult<FileResponse>.Fail(fileDTO.Error));

            // Загрузка файла в хранилище
            await using var fileStream = file.OpenReadStream();
            var storageResult = await _storageService.LoadFileAsStream(fileStream, fileDTO.Value);
            if (!storageResult.IsSuccess)
                return BadRequest(ApiResult<FileResponse>.Fail(storageResult.Error));

            // Сохранение информации в базе данных
            var dbResult = await _filesService.UploadFile(storageResult.Value);
            if (!dbResult.IsSuccess)
            {
                await _storageService.DeleteFileAsync(fileDTO.Value.Id);
                return BadRequest(ApiResult<FileResponse>.Fail(dbResult.Error));
            }

            // Оповещение через SignalR
            await _hubContext.Clients.All.SendAsync("FileLoaded", storageResult.Value);

            var fileResponse = new FileResponse(fileDTO.Value.Id, fileDTO.Value.Name, fileDTO.Value.Size);
            return Ok(ApiResult<FileResponse>.Success(fileResponse));
        }

        [HttpDelete("delete/{id:Guid}")]
        public async Task<ActionResult<ApiResult<DeleteFileResponse>>> DeleteFile(Guid id)
        {
            var deleteResult = await _storageService.DeleteFileAsync(id);
            if(!deleteResult.IsSuccess)
                return BadRequest(ApiResult<DeleteFileResponse>.Fail(deleteResult.Error));

            var deletedFileName = await _filesService.DeleteFile(id);
            if (!deletedFileName.IsSuccess)
                return BadRequest(ApiResult<DeleteFileResponse>.Fail(deletedFileName.Error));

            // Оповещение через SignalR
            await _hubContext.Clients.All.SendAsync("FileDeleted", deleteResult.Value.Id);

            return Ok(ApiResult<DeleteFileResponse>.Success(new DeleteFileResponse(deletedFileName.Value)));
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

        [HttpPut("rename/{id}")]
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

        [HttpPut("move/{id}")]
        public async Task<ActionResult> MoveFile(Guid id, [FromBody] MoveFileRequest dto)
        {
            var oldfolderResult = await _filesService.GetFileById(id);
            if(!oldfolderResult.IsSuccess)
                return NotFound();

            var result = await _storageService.MoveFile(id, dto.FolderId);
            if(!result.IsSuccess)
                return BadRequest(result.Error);

            var updated = await _filesService.MoveFile(id, result.Value, dto.FolderId);
            if (!updated.IsSuccess)
            {
                await _storageService.MoveFile(id, oldfolderResult.Value.FolderId);
                return BadRequest(updated.Error);
            }
            return Ok(updated.Value);
        }
    }
}
