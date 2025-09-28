namespace FileCloud.Contracts.Responses
{
    public record UserResponse(
        Guid Id,
        string Login,
        string Email,
        DateTime CreatedAt);
}
