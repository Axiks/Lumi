using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Interfaces
{
    public interface IUserService
    {
        public Task<UserModel> RegisterUser(UserRegisterModel user);
        public Task<UserModel> SignInUser(long telegramId);
        public Task<UserModel> GetUser(Guid userId);
        public Task<List<UserModel>> FindByUsername(string usernme);
    }
}
