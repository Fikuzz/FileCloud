namespace FileCloud.Core.Contracts.Requests
{
    public record LoginRequest(
        string Login,
        string Password 
    );
}
