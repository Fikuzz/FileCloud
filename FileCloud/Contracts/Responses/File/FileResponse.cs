namespace FileCloud.Contracts.Responses.File
{
    public record FileResponse(
        Guid Id,
        string Name,
        long? Size);
}
