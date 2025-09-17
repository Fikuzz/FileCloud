using FileCloud.Core.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FileCloud.Application.Services
{
    public class JwtService : IJwtService
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly double _expiryInMinutes;

        // Внедряем конфигурацию, чтобы получить доступ к настройкам из appsettings.json
        public JwtService(IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("Jwt");

            _secret = jwtSection["Secret"] ?? throw new ArgumentNullException("Jwt:Secret is not configured");
            _issuer = jwtSection["Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer is not configured");
            _audience = jwtSection["Audience"] ?? throw new ArgumentNullException("Jwt:Audience is not configured");

            // Безопасно парсим значение времени жизни
            if (!double.TryParse(jwtSection["ExpiryInMinutes"], out _expiryInMinutes))
            {
                _expiryInMinutes = 60; // Значение по умолчанию
            }
        }

        public string GenerateToken(Guid userId, string login, string email)
        {
            // 1. Создаем "утверждения" (Claims) для токена. Это полезная нагрузка.
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, login),
            new Claim(ClaimTypes.Email, email)
            // Сюда можно добавить другие claims, например, роль (ClaimTypes.Role, "Admin")
        };

            // 2. Создаем секретный ключ из строки-секрета
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));

            // 3. Создаем учетные данные для подписи токена этим ключом по алгоритму HMACSHA256
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            // 4. Описываем сам токен: кто выпустил, для кого, какие claims, когда истекает, и чем подписан.
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expiryInMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = creds
            };

            // 5. Создаем обработчик токенов и генерируем токен
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // 6. Записываем токен в компактную строку формата JWT
            return tokenHandler.WriteToken(token);
        }
    }
}
