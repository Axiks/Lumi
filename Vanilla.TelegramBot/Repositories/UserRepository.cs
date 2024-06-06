using Microsoft.EntityFrameworkCore;
using Vanilla.TelegramBot.Entityes;
using Vanilla.TelegramBot.Helpers;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public UserRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserCreateResponseModel> AddUserAsync(UserCreateRequestModel user)
        {
            var userEntity = new UserEntity
            {
                UserId = user.UserId,
                TelegramId = user.TelegramId,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
            };
            await _dbContext.AddAsync(userEntity);
            await _dbContext.SaveChangesAsync();

            return MapperHelper.UserEntityToUserCreateResponseModel(userEntity);
        }

        public async Task<UserCreateResponseModel?> GetUserAsync(Guid userId)
        {
            var userEntity = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            if (userEntity is null) return null;
            return MapperHelper.UserEntityToUserCreateResponseModel(userEntity);
        }

        public async Task<UserCreateResponseModel?> GetUserAsync(long telegramId)
        {
            var userEntity = await _dbContext.Users.FirstOrDefaultAsync(x => x.TelegramId == telegramId);
            if (userEntity is null) return null;
            return MapperHelper.UserEntityToUserCreateResponseModel(userEntity);
        }

        public async Task<List<UserCreateResponseModel>> GetUsersAsync(string username)
        {
            var usersEntity = _dbContext.Users.Where(x => x.Username == username).ToList();
            var users = new List<UserCreateResponseModel>();
            foreach (var userEntity in usersEntity)
            {
                users.Add(MapperHelper.UserEntityToUserCreateResponseModel(userEntity));
            }
            return users;
        }

        public async void RemoveUserAsync(Guid userId)
        {
            var user = await _dbContext.Users.FirstAsync(x => x.UserId == userId);
            _dbContext.Remove(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserCreateResponseModel> UpdateUserAsync(UserCreateRequestModel user)
        {
            var userEntity = await _dbContext.Users.FirstAsync(x => x.UserId == user.UserId);
            if (user.Username is not null) userEntity.Username = user.Username;
            await _dbContext.SaveChangesAsync();
            return MapperHelper.UserEntityToUserCreateResponseModel(userEntity);
        }
    }
}
