using System.ComponentModel.DataAnnotations;

namespace FileCloud.Contracts.Requests.File
{
    public record UploadFileRequest(
        Guid FolderId);
}
