using FileCloud.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FileCloud.Application.Services
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?
                    .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return Guid.TryParse(userId, out var guid) ? guid : null;
            }
        }

        public string? Login => _httpContextAccessor.HttpContext?
            .User.FindFirst(ClaimTypes.Name)?.Value;

        public string? Email => _httpContextAccessor.HttpContext?
            .User.FindFirst(ClaimTypes.Email)?.Value;

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?
            .User.Identity?.IsAuthenticated ?? false;
    }
}
