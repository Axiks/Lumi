using System;
using Vanilla_App.Module;

namespace Vanilla_App.Services.Users
{
    public interface IUserService
    {
        Task<UserModel> GetUser(Guid userId);
        Task<List<UserModel>> GetUsers();
        Task<UserModel> CreateUser(UserCreateRequestModel update);
        Task<UserModel> UpdateUser(Guid userId, UserUpdateRequestModel update);
        Task<bool> DeleteUser(Guid userId);

        Task<ProfileImage> AddProfileImage(Guid userId, DownloadFileRequestModel image);
        Task<bool> RemoveProfileImage(Guid userId, Guid imageId);
    }
}
