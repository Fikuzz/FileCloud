using FileCloud.Core.Models;

namespace FileCloud.Core.Abstractions
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
        Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    }
}
