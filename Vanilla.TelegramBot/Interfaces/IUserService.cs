using Vanilla.OAuth.Models;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Interfaces
{
    public interface IUserService
    {
        public Task<UserModel> RegisterUser(UserRegisterModel user);
        public Task<UserModel> SignInUser(long telegramId);
        public Task<UserModel> GetUser(Guid userId);
        public Task<UserModel> UpdateUser(long userTgId, Models.UserUpdateRequestModel user);
        public Task<List<UserModel>> FindByUsername(string usernme);
    }
}
