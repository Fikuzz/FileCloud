using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileCloud.Core.Models
{
    public class User
    {
        private User(Guid id, string login, string passwordHash, string email, DateTime createdAt)
        {
            Id = id;
            Login = login;
            PasswordHash = passwordHash;
            Email = email;
            CreatedAt = createdAt;
        }

        public Guid Id { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }

        // Только корневая папка
        public Folder RootFolder { get; set; }

        public static Result<User> Create(Guid? id, string login, string passwordHash, string email, DateTime createdAt)
        {
            if (string.IsNullOrWhiteSpace(login))
                return Result<User>.Fail("Логин не может быть пустым");

            if (login.Length < 3)
                return Result<User>.Fail("Логин должен содержать минимум 3 символа");

            if (login.Length > 50)
                return Result<User>.Fail("Логин слишком длинный");

            // Валидация email
            if (string.IsNullOrWhiteSpace(email))
                return Result<User>.Fail("Email не может быть пустым");

            if (!IsValidEmail(email))
                return Result<User>.Fail("Некорректный формат email");

            // Валидация passwordHash
            if (string.IsNullOrWhiteSpace(passwordHash))
                return Result<User>.Fail("Password hash не может быть пустым");


            var user = new User(
                id: id.HasValue ? id.Value : Guid.NewGuid(),
                login: login.Trim(),
                passwordHash: passwordHash,
                email: email.Trim().ToLower(),
                createdAt: DateTime.UtcNow
            );

            return Result<User>.Success(user);
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
