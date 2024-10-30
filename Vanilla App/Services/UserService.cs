using Vanilla.Data.Entities;
using Vanilla_App.Interfaces;
using Vanilla_App.Models;

namespace Vanilla_App.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<ProjectModel>> UserProjectsGetAsync(UserModel user)
        {
            return await _userRepository.GetProjectsAsync(user.Id);
        }

        public UserModel GetUser(Guid userId)
        {
            var entity = _userRepository.Get(userId);
            var user = EntityToModelMapper(entity);
            return user;
        }

        public UserModel CreateUser(UserCreateRequestModel create)
        {
            var entity = _userRepository.Create(create);
            var user = EntityToModelMapper(entity);
            return user;
        }

        public UserModel UpdateUser(Guid userId, UserUpdateRequestModel update)
        {
            var entity = _userRepository.Update(userId, update);
            var user = EntityToModelMapper(entity);
            return user;
        }

        private static UserModel EntityToModelMapper(UserEntity entity) => new UserModel
        {
            Id = entity.Id,
            About = entity.About,
            Links = entity.Links,
            IsRadyForOrders = entity.IsRadyForOrders
        };
    }
}
