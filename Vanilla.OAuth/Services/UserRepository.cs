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
            bool IsNicknameExist = _dbContext.Users.Any(x => x.Nickname == createUser.NickName);
            if (IsNicknameExist is true) throw new ArgumentException("A user with this nickname already exists");

            var userEntity = new UserEntity
            {
                Nickname = createUser.NickName
            };
            await _dbContext.Users.AddAsync(userEntity);
            await _dbContext.SaveChangesAsync();

            return MappingHelper.UserEntityToBasicUserModel(userEntity);
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var userEntity = await _dbContext.Users.FirstAsync(x => x.Id == userId);
            _dbContext.Users.Remove(userEntity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<BasicUserModel> GetUserAsync(Guid userId)
        {
            var userEntity = await _dbContext.Users.FirstAsync(x => x.Id == userId);
            return MappingHelper.UserEntityToBasicUserModel(userEntity);
        }

        public async Task<BasicUserModel?> GetUserOrDefaultAsync(Guid userId)
        {
            var userEntity = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (userEntity == null) return null;
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
