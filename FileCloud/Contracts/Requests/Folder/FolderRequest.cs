using System.ComponentModel.DataAnnotations;

namespace FileCloud.Contracts.Requests.Folder
{
    public record FolderRequest(
        [Required]
        string Name,
        Guid? parentId);
}
