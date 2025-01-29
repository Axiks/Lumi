using Vanilla.OAuth.Models;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Interfaces
{
    public interface IUserService
    {
        public Task<UserModel> RegisterUser(UserRegisterModel user);
        public Task<UserModel> SignInUserAsync(long telegramId);
        public Task<UserModel> GetUserAsync(Guid userId);
        public Task<List<UserModel>> GetUsersAsync();
        public Task<UserModel> UpdateUserAsync(long userTgId, Models.UserUpdateRequestModel user);
        public Task<List<UserModel>> FindByUsernameAsync(string usernme);
        public Task DeleteUserAsync(Guid userId);
    }
}
