using FileCloud.Contracts;
using FileCloud.Core.Abstractions;
using FileCloud.Core.Models;
using FileCloud.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FileCloud.Controllers
{
    [ApiController]
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
        public async Task<ActionResult<List<FolderResult>>> GetAllFolders()
        {
            var foldersResult = await _folderService.GetAllFolders();

            var response = foldersResult.Select(r => new FolderResult
            {
                Response = r.IsSuccess ? new FolderResponse(r.Value.Id, r.Value.Name, r.Value.ParentId) : null,
                Error = r.IsSuccess ? null : r.Error
            }).ToList();

            return Ok(response);
        }

        [HttpGet("{id:Guid}")]
        public async Task<ActionResult<FolderResponse>> GetFolder(Guid id)
        {
            var FolderResult = await _folderService.GetFolder(id);
            if (!FolderResult.IsSuccess)
            {
                return NotFound(FolderResult.Error);
            }

            return Ok(FolderResult.Value);
        }

        [HttpGet("child/{id:guid}")]
        public async Task<ActionResult<List<FolderResponse>>> GetChildFolders(Guid id)
        {
            var result = await _folderService.GetChildFolder(id);
            if (!result.IsSuccess)
                return NotFound(result.Error);

            return Ok(result.Value);
        }

        [HttpPost]
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
            return Ok(result.Value);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteFolder(Guid id)
        {
            var folderResult = await _storageService.DeleteFolderCascadeAsync(id);
            if (!folderResult.IsSuccess)
                return BadRequest(folderResult.Error);

            return Ok();
        }
    }
}
