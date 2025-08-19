namespace FileCloud.Contracts
{
    public record FileResponse(
        Guid id,
        string Name,
        long? Size,
        string Path);
}
