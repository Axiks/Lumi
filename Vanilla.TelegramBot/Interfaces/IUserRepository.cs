using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Interfaces
{
    public interface IUserRepository
    {
        public Task<UserCreateResponseModel> AddUserAsync(UserCreateRequestModel user);
        public Task<UserCreateResponseModel?> GetUserAsync(Guid userId);
        public Task<UserCreateResponseModel?> GetUserAsync(long telegramId);
        public void RemoveUserAsync(Guid userId);
        public Task<UserCreateResponseModel> UpdateUserAsync(UserCreateRequestModel user);

    }
}
