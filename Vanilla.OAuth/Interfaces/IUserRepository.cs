using Vanilla.OAuth.Models;

namespace Vanilla.OAuth.Interfaces
{
    public interface IUserRepository
    {
        public Task<BasicUserModel> GetUserAsync(Guid userId);
        public Task<BasicUserModel> CreateUserAsync(UserCreateRequestModel createUser);
        public Task<BasicUserModel> UpdateUserAsync(UserUpdateRequestModel updateUser);
        public void DeleteUser(Guid userId);

    }
}
