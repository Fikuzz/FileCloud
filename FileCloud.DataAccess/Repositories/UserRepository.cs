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

        public async Task<Result<List<User>>> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .AsNoTracking()
                    .ToListAsync();

                var models = users.Select(u => UserMapper.ToModel(u).Value).ToList();

                return Result<List<User>>.Success(models);
            }
            catch (Exception ex)
            {
                return Result<List<User>>.Fail($"Error retrieving users: {ex.Message}");
            }
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

        public async Task<Result> DeleteAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Удаляем связанные данные
                await _context.Files
                    .Where(f => f.Folder.OwnerId == id)
                    .ExecuteDeleteAsync();

                await _context.Folders
                    .Where(f => f.OwnerId == id)
                    .ExecuteDeleteAsync();

                // Удаляем пользователя
                var rowsAffected = await _context.Users
                    .Where(u => u.Id == id)
                    .ExecuteDeleteAsync();

                await transaction.CommitAsync();

                return rowsAffected > 0
                    ? Result.Success()
                    : Result.Fail("User not found");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result.Fail($"Delete failed: {ex.Message}");
            }
        }

        public async Task<Result> EndSession(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Result.Fail("User not Exist");

            user.TokensValidAfter = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Result.Success();
        }
    }
}