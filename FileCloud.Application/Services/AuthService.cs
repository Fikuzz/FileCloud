using FileCloud.Core;
using FileCloud.Core.Abstractions;
using FileCloud.Core.Models;

namespace FileCloud.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public AuthService(IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
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
            // 3. TODO: ВРЕМЕННО - простая проверка пароля
            // ЗДЕСЬ НУЖНО БУДЕТ ДОБАВИТЬ ПРОВЕРКУ ХЕША ПАРОЛЯ!
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Result<AuthResponse>.Fail("Неверный логин или пароль");
            }

            // 4. Генерируем JWT-токен
            var token = _jwtService.GenerateToken(user.Id, user.Login, user.Email);

            // 5. Возвращаем ответ с токеном
            return Result<AuthResponse>.Success(new AuthResponse(user.Id, user.Login, user.Email, token));
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
                passwordHash, // Сохраняем ХЕШ, а не plain password
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

            // 5. Генерируем токен и возвращаем ответ
            var token = _jwtService.GenerateToken(user.Id, user.Login, user.Email);
            return Result<AuthResponse>.Success(new AuthResponse(user.Id, user.Login, user.Email, token));
        }
    }
}
