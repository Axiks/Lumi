using Vanilla.OAuth.Models;

namespace Vanilla.OAuth.Interfaces
{
    public interface IUserRepository
    {
        public Task<BasicUserModel> GetUserAsync(Guid userId);
        public Task<BasicUserModel?> GetUserOrDefaultAsync(Guid userId);
        public Task<BasicUserModel> CreateUserAsync(UserCreateRequestModel createUser);
        public Task<BasicUserModel> UpdateUserAsync(Guid userId, UserUpdateRequestModel updateUser);
        public Task DeleteUserAsync(Guid userId);

    }
}
