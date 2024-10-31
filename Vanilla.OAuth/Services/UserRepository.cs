using Microsoft.EntityFrameworkCore;
using Vanilla.OAuth.Entities;
using Vanilla.OAuth.Helpers;
using Vanilla.OAuth.Interfaces;
using Vanilla.OAuth.Models;

namespace Vanilla.OAuth.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public UserRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BasicUserModel> CreateUserAsync(UserCreateRequestModel createUser)
        {
            var userEntity = new UserEntity
            {
                Nickname = createUser.NickName
            };
            await _dbContext.Users.AddAsync(userEntity);
            await _dbContext.SaveChangesAsync();

            return MappingHelper.UserEntityToBasicUserModel(userEntity);
        }

        public async void DeleteUser(Guid userId)
        {
            var userEntity = await _dbContext.Users.FirstAsync(x => x.Id == userId);
            _dbContext.Users.Remove(userEntity);
        }

        public async Task<BasicUserModel> GetUserAsync(Guid userId)
        {
            var userEntity = await _dbContext.Users.FirstAsync(x => x.Id == userId);
            return MappingHelper.UserEntityToBasicUserModel(userEntity);
        }

        public async Task<BasicUserModel> UpdateUserAsync(Guid userId, UserUpdateRequestModel updateUser)
        {
            var userEntity = await _dbContext.Users.FirstAsync(x => x.Id == userId);
            if (updateUser.NickName is not null) userEntity.Nickname = updateUser.NickName;
            _dbContext.Update(userEntity);
           await  _dbContext.SaveChangesAsync();

            return MappingHelper.UserEntityToBasicUserModel(userEntity);
        }
    }
}
