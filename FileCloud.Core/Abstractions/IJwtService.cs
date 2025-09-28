namespace FileCloud.Core.Abstractions
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, string login, string email);
    }
}
