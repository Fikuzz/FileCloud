using FileCloud.Contracts.Responses.Folder;
using FileCloud.Core;
using FileCloud.Core.Abstractions;
using FileCloud.Core.Contracts.Requests;
using FileCloud.Core.Contracts.Responses;
using FileCloud.Core.Models;

namespace FileCloud.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserContext _userContext;
        private readonly IUserRepository _userRepository;
        private readonly IFolderService _folderService;
        private readonly IJwtService _jwtService;

        public AuthService(IUserRepository userRepository,  IJwtService jwtService, IFolderService folderService, IUserContext userContext)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _folderService = folderService;
            _userContext = userContext;
        }
        public async Task<Result<List<User>>> GetUsers()
        {
            return await _userRepository.GetUsers();
        }
        public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
        {
            // 1. Находим пользователя по логину
            var userResult = await _userRepository.GetByLoginAsync(request.Login);

            // 2. Если пользователь не найден - возвращаем null
            if (!userResult.IsSuccess)
            {
                return Result<AuthResponse>.Fail(userResult.Error);
            }
            var user = userResult.Value;
            // Проверка Хеша Пароля!
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Result<AuthResponse>.Fail("Неверный логин или пароль");
            }

            // 4. Генерируем JWT-токен
            var token = _jwtService.GenerateToken(user.Id, user.Login, user.Email);

            // 5. Возвращаем ответ с токеном
            return Result<AuthResponse>.Success(new AuthResponse(user.Id, user.Login, user.Email, token,
                new FolderResponse(
                    user.RootFolder.Id,
                    user.RootFolder.Name,
                    null)));
        }
        public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
        {
            // 1. Проверяем, нет ли уже пользователя с таким логином
            var existingUser = await _userRepository.GetByLoginAsync(request.Login);
            if (existingUser.IsSuccess)
            {
                return Result<AuthResponse>.Fail("Пользователь с таким логином уже существует");
            }

            // 2. Хешируем пароль
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 3. Создаем нового пользователя
            var userResult = User.Create
            (
                Guid.NewGuid(),
                request.Login,
                passwordHash,
                request.Email,
                DateTime.UtcNow
            );
            if (!userResult.IsSuccess)
            {
                return Result<AuthResponse>.Fail(userResult.Error);
            }
            var user = userResult.Value;

            // 4. Сохраняем в базу
            await _userRepository.CreateAsync(user);

            // 5. Создаем корневую папку для пользователя
            var folderResult = await _folderService.CreateRootFolder(user.Login, user.Id);
            if(!folderResult.IsSuccess)
                return Result<AuthResponse>.Fail(folderResult.Error);

            user.RootFolder = folderResult.Value;

            // 6. Генерируем токен и возвращаем ответ
            var token = _jwtService.GenerateToken(user.Id, user.Login, user.Email);
            return Result<AuthResponse>.Success(new AuthResponse(user.Id, user.Login, user.Email, token,
                new FolderResponse(
                    user.RootFolder.Id,
                    user.RootFolder.Name,
                    null)));
        }
        public async Task<Result> DeleteAsync()
        {
            if (!_userContext.IsAuthenticated || _userContext.UserId == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            return await _userRepository.DeleteAsync(_userContext.UserId.Value);
        }
        public async Task<Result> EndSession()
        {
            if (!_userContext.IsAuthenticated || _userContext.UserId == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            return await _userRepository.EndSession(_userContext.UserId.Value);
        }
    }
}
