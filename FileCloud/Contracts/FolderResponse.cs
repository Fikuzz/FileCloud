namespace FileCloud.Contracts
{
    public record FolderResponse(
        Guid Id,
        string Name,
        Guid? ParentId);
}
