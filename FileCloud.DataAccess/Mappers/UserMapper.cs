using FileCloud.Core;
using FileCloud.Core.Models;
using FileCloud.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.DataAccess.Mappers
{
    public static class UserMapper
    {
        public static UserEntity ToEntity(User user)
        {
            return new UserEntity
            {
                Id = user.Id,
                Login = user.Login,
                PasswordHash = user.PasswordHash,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };
        }

        public static Result<User> ToModel(UserEntity entity)
        {
            return User.Create(
                entity.Id,
                entity.Login,
                entity.PasswordHash,
                entity.Email,
                entity.CreatedAt
            );
        }
    }
}
