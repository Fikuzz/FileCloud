namespace FileCloud.Core.Models;

public record AuthResponse(
    Guid UserId,
    string Login,
    string Email,
    string Token // Сюда будет подставлен сгенерированный JWT
);