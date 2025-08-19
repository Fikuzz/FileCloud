namespace FileCloud.Contracts
{
    public record FolderRequest(
        string Name,
        Guid? parentId);
}
