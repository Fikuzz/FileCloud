using FileCloud.Contracts.Responses.File;
using FileCloud.Contracts.Responses.Folder;

namespace FileCloud.Contracts.Responses
{
    public class ContentResponse
    {
        public List<FileResponse> Files { get; set; } = new();
        public List<FolderResponse> Folders { get; set; } = new();
    }
}
