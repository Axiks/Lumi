﻿using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Interfaces
{
    public interface IUserRepository
    {
        public Task<UserCreateResponseModel> AddUserAsync(UserCreateRequestModel user);
        public Task<UserCreateResponseModel?> GetUserOrDefaultAsync(Guid userId);
        public Task<UserCreateResponseModel?> GetUserAsync(long telegramId);
        public Task<List<UserCreateResponseModel>> GetUsersAsync(string username);
        public Task<List<UserCreateResponseModel>> GetUsersAsync();
        public void DeleteUserAsync(Guid userId);
        public Task<UserCreateResponseModel> UpdateUserAsync(UserCreateRequestModel user);

    }
}
