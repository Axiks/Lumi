using System;
using Vanilla_App.Module;

namespace Vanilla_App.Services.Users
{
    public interface IUserService
    {
        Task<UserModel> GetUserOrDefaultAsync(Guid userId);
        Task<List<UserModel>> GetUsersAsync();
        Task<UserModel> CreateUserAsync(UserCreateRequestModel update);
        Task<UserModel> UpdateUserAsync(Guid userId, UserUpdateRequestModel update);
        Task<bool> DeleteUserAsync(Guid userId);

        Task<ProfileImage> AddProfileImageAsync(Guid userId, DownloadFileRequestModel image);
        Task<bool> RemoveProfileImageAsync(Guid userId, Guid imageId);
    }
}
