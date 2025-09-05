namespace FileCloud.Contracts.Responses.File
{
    public record RenameFileResponse(
        Guid Id,
        string NewName);
}
