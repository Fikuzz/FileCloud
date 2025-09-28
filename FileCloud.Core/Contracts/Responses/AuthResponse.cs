using FileCloud.Contracts.Responses.Folder;

namespace FileCloud.Core.Contracts.Responses;

public record AuthResponse(
    Guid UserId,
    string Login,
    string Email,
    string Token, // Сюда будет подставлен сгенерированный JWT
    FolderResponse RootFolder
);