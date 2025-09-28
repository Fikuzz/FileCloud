using FileCloud.Contracts;
using FileCloud.Contracts.Requests.Folder;
using FileCloud.Contracts.Responses;
using FileCloud.Contracts.Responses.File;
using FileCloud.Contracts.Responses.Folder;
using FileCloud.Core.Abstractions;
using FileCloud.Core.Models;
using FileCloud.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FileCloud.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/folder")]
    public class FolderController : Controller
    {
        private readonly IFolderService _folderService;
        private readonly IStorageService _storageService;
        private readonly IHubContext<FileHub> _hubContext;
        public FolderController(IFolderService folderService, IHubContext<FileHub> hubContext, IStorageService storageService)
        {
            _folderService = folderService;
            _hubContext = hubContext;
            _storageService = storageService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ApiResult<FolderResponse>>>> GetAllFolders()
        {
            var foldersResult = await _folderService.GetAllFolders();

            var response = foldersResult.Select(r => new ApiResult<FolderResponse>
            {
                Response = r.IsSuccess ? new FolderResponse(r.Value.Id, r.Value.Name, r.Value.ParentId) : null,
                Error = r.IsSuccess ? null : r.Error
            }).ToList();

            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResult<FolderResponse>>> GetFolder(Guid id)
        {
            var FolderResult = await _folderService.GetFolder(id);
            if (!FolderResult.IsSuccess)
            {
                return NotFound(ApiResult<FolderResponse>.Fail(FolderResult.Error));
            }

            var response = new FolderResponse(
                FolderResult.Value.Id,
                FolderResult.Value.Name,
                FolderResult.Value.ParentId
            );

            return Ok(ApiResult<FolderResponse>.Success(response));
        }

        [HttpGet("{id:guid}/childs")]
        public async Task<ActionResult<ApiResult<ContentResponse>>> GetFolderChilds(Guid id)
        {
            var foldersResult = await _folderService.GetSubFolder(id);
            if (!foldersResult.IsSuccess)
                return NotFound(ApiResult<ContentResponse>.Fail(foldersResult.Error));

            var filesResult = await _folderService.GetFiles(id);
            if (!filesResult.IsSuccess)
                return NotFound(ApiResult<ContentResponse>.Fail(filesResult.Error));

            var response = new ContentResponse
            {
                Files = filesResult.Value.Select(r => new FileResponse(r.Id, r.Name, r.Size)).ToList(),
                Folders = foldersResult.Value.Select(r => new FolderResponse(r.Id, r.Name, r.ParentId)).ToList()
            };

            return Ok(ApiResult<ContentResponse>.Success(response));
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateFolder([FromBody] FolderRequest request)
        {
            var folderResult = await _storageService.CreateNewFolder(request.Name, request.parentId);
            if(!folderResult.IsSuccess)
                return BadRequest(folderResult.Error);

            var result = await _folderService.CreateFolder(request.Name, request.parentId);
            if (!result.IsSuccess)
            {
                _storageService.DeleteFolderByPath(folderResult.Value);
                return BadRequest(result.Error);
            }

            await _hubContext.Clients
                .Group(request.parentId.ToString())
                .SendAsync("FolderCreated", result.Value.Id);
            return Ok(result.Value);
        }

        [HttpDelete("delete/{id:guid}")]
        public async Task<ActionResult<DeleteFolderResponse>> DeleteFolder(Guid id)
        {
            var folderResult = await _storageService.DeleteFolderCascadeAsync(id);
            if (!folderResult.IsSuccess)
                return BadRequest(folderResult.Error);

            var response = new DeleteFolderResponse(folderResult.Value.Name);
            // Оповещение через SignalR
            await _hubContext.Clients.All
                .SendAsync("FolderDeleted", folderResult.Value.Id);
            return Ok(response);
        }

        [HttpPut("rename/{id:guid}")]
        public async Task<ActionResult<FolderResponse>> RenameFolder(Guid id, [FromBody] RenameFolderRequest request)
        {
            var oldFolderResult = await _folderService.GetFolder(id);
            if(!oldFolderResult.IsSuccess)
                return NotFound(oldFolderResult.Error);

            var folderResult = await _storageService.RenameFolder(id, request.NewName);
            if (!folderResult.IsSuccess)
                return BadRequest(folderResult.Error);

            var DbResult = await _folderService.RenameFolder(id, request.NewName);
            if (!DbResult.IsSuccess)
            {
                await _storageService.RenameFolder(id, oldFolderResult.Value.Name);
                return BadRequest(DbResult.Error);
            }
            
            var responseResult = await _folderService.GetFolder(DbResult.Value);
            if (!responseResult.IsSuccess)
                return NotFound(responseResult.Error);

            var response = new FolderResponse(responseResult.Value.Id, responseResult.Value.Name, responseResult.Value.ParentId);
            await _hubContext.Clients
                .Groups(response.ParentId.ToString())
                .SendAsync("FolderRenamed", new FolderRenameResponse(id, response.Name));
            return Ok(response);
        }
        [HttpPut("move/{id:guid}")]
        public async Task<ActionResult<FolderResponse>> MoveFolder(Guid id, [FromBody] MoveFolderRequest request)
        {
            var oldFolderResult = await _folderService.GetFolder(id);
            if (!oldFolderResult.IsSuccess)
                return BadRequest(oldFolderResult.Error);

            var folderResult = await _storageService.MoveFolder(id, request.FolderId);
            if (!folderResult.IsSuccess)
                return BadRequest(folderResult.Error);

            var DbResult = await _folderService.MoveFolder(id, request.FolderId);
            if (!DbResult.IsSuccess)
            {
                await _storageService.MoveFolder(id, oldFolderResult.Value.ParentId);
                return BadRequest(DbResult.Error);
            }

            var responseResult = await _folderService.GetFolder(DbResult.Value);
            if (!responseResult.IsSuccess)
                return NotFound(responseResult.Error);

            var response = new FolderResponse(responseResult.Value.Id, responseResult.Value.Name, responseResult.Value.ParentId);
            await _hubContext.Clients
                .Groups(oldFolderResult.Value.ParentId.ToString())
                .SendAsync("FolderDeleted", response.Id);
            await _hubContext.Clients
                .Group(response.Id.ToString())
                .SendAsync("FolderCreated", id);
            return Ok(response);
        }
    }
}
