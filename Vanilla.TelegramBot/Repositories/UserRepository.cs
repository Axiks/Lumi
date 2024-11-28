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
                LanguageCode = user.LanguageCode,
                IsHasProfile = user.IsHasProfile
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
            var userEntity = await _dbContext.Users.Include(x => x.Images).FirstOrDefaultAsync(x => x.TelegramId == telegramId);
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

        public async Task<List<UserCreateResponseModel>> GetUsersAsync()
        {
            var usersEntity = _dbContext.Users.ToList();
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
            if (user.LanguageCode is not null) userEntity.LanguageCode = user.LanguageCode;
            if (user.Username is not null) userEntity.Username = user.Username;
            if (user.FirstName is not null) userEntity.FirstName = user.FirstName;
            if (user.LastName is not null) userEntity.LastName = user.LastName;
            userEntity.IsHasProfile = user.IsHasProfile;

            if (user.Images is not null)
            {
                userEntity.Images = new List<ImagesEntity>();
                foreach (var image in user.Images)
                {
                    userEntity.Images.Add(new ImagesEntity { TgMediaId = image.TgMediaId });
                }
            } 

            _dbContext.Update(userEntity);
            await _dbContext.SaveChangesAsync();
            return MapperHelper.UserEntityToUserCreateResponseModel(userEntity);
        }
    }
}
