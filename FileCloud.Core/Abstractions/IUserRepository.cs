using FileCloud.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Core.Abstractions
{
    public interface IUserRepository
    {
        public Task<Result<List<User>>> GetUsers();
        public Task<Result> CreateAsync(User user);
        public Task<Result<User>> GetByLoginAsync(string login);
        public Task<Result> UserExistAsync(string login);
        public Task<Result> DeleteAsync(Guid id);
        public Task<Result> EndSession(Guid userId);
    }
}
