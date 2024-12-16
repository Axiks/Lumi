using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla_App.Services.Users
{
    public interface IUserService
    {
        Task<UserModel> GetUser(Guid userId);
        Task<List<UserModel>> GetUsers();
        Task<UserModel> CreateUser(UserCreateRequestModel update);
        Task<UserModel> UpdateUser(Guid userId, UserUpdateRequestModel update);
        Task<bool> DeleteUser(Guid userId);
    }
}
