namespace FileCloud.Contracts.Responses.Folder
{
    public record FolderResponse(
        Guid Id,
        string Name,
        Guid? ParentId);
}
