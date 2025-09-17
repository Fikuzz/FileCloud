using FileCloud.Core;
using FileCloud.Core.Abstractions;
using FileCloud.Core.Models;
using FileCloud.DataAccess.Mappers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly FileCloudDbContext _context;
        public UserRepository(FileCloudDbContext context)
        {
            _context = context;
        }

        public async Task<Result> CreateAsync(User user)
        {
            var userEntity = UserMapper.ToEntity(user);
            await _context.Users.AddAsync(userEntity);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result<User>> GetByLoginAsync(string login)
        {
            // Ищем пользователя по логину (регистронезависимо)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Login.ToLower() == login.ToLower());

            if (user == null)
                return Result<User>.Fail("Пользователя с таким логином не существует");

            return UserMapper.ToModel(user);
        }

        public async Task<Result> UserExistAsync(string login)
        {
            // Проверяем, существует ли пользователь с таким логином
            var hasUser = await _context.Users
                .AnyAsync(u => u.Login.ToLower() == login.ToLower());

            if (hasUser)
                return Result.Success();
            else
                return Result.Fail("Пользователя с таким логином не существует");
        }
    }
}
