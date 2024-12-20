using System.Collections.Generic;
using Vanilla.Data.Entities;
using Vanilla_App.Module;
using Vanilla_App.Services.Projects;
using Vanilla_App.Services.Projects.Repository;
using Vanilla_App.Services.Users;
using Vanilla_App.Services.Users.Repository;

namespace Vanilla_App.Services
{
    public class UserService(IUserRepository _coreUserRepository, IProjectRepository _projectRepository, Vanilla.OAuth.Services.UserRepository _oauthUserService) : IUserService
    {
        public async Task<List<UserModel>> GetUsers()
        {
            var users = new List<UserModel>();

            var entities = _coreUserRepository.GetAll();
            foreach (var entity in entities) {
                var OAuthUser = await _oauthUserService.GetUserAsync(entity.Id);
                users.Add(ToUserModelMapper(entity, OAuthUser));
            }

            return users;
        }

        public async Task<UserModel> GetUser(Guid userId)
        {
            // oauth
            var OAthUser = await _oauthUserService.GetUserAsync(userId);
            var entity = _coreUserRepository.Get(userId);

            var user = ToUserModelMapper(entity, OAthUser);
            return user;
        }

        public async Task<UserModel> CreateUser(UserCreateRequestModel create)
        {
            // oauth
            var OAthUser = await _oauthUserService.CreateUserAsync(new Vanilla.OAuth.Models.UserCreateRequestModel
            {
                NickName = create.Nickname,
            });

            // user
            var entity = _coreUserRepository.Create(new CoreUserCreateRequestModel
            {
                UserId = OAthUser.Id,
                About = create.About,
                Links = create.Links,
                IsRadyForOrders = create.IsRadyForOrders ?? false,
            });

            return ToUserModelMapper(entity, OAthUser);
        }

        public async Task<UserModel> UpdateUser(Guid userId, UserUpdateRequestModel update)
        {
            // oauth
            var OAthUser = await _oauthUserService.GetUserAsync(userId);

            var entity = _coreUserRepository.Update(userId, new CoreUserUpdateRequestModel
            {
                About = update.About,
                Links = update.Links,
                IsRadyForOrders = update.IsRadyForOrders,
            });

            return ToUserModelMapper(entity, OAthUser);
        }

        public async Task<bool> DeleteUser(Guid userId)
        {
            // oauth
            _oauthUserService.DeleteUser(userId);

            //Core
            // delete the all user projects
            foreach (var project in _coreUserRepository.GetProjectsAsync(userId).Result)
            {
                _projectRepository.Delete(project.Id);
            }

            _coreUserRepository.Delete(userId);

            return true;
        }

        public async Task<List<ProjectModel>> UserProjectsGetAsync(UserModel user)
        {
            return await _coreUserRepository.GetProjectsAsync(user.Id);
        }

        private static UserModel ToUserModelMapper(UserEntity entity, Vanilla.OAuth.Models.BasicUserModel OAuthUser) => new UserModel
        {
            Id = entity.Id,
            About = entity.About,
            Links = entity.Links,
            IsRadyForOrders = entity.IsRadyForOrders,
            Nickname = OAuthUser.Nickname,
            ProfileImages = ImagesEntityToProfileImagesMapper(entity.ProfileImages),
        };

        private static List<ProfileImage> ImagesEntityToProfileImagesMapper(List<ImageEntity>? imagesEntity)
        {
            if(imagesEntity is null) return new List<ProfileImage> { };

            var images = new List<ProfileImage>();

            foreach (var imageEntity in imagesEntity)
            {
                images.Add(
                    new ProfileImage
                    {
                        Id = imageEntity.Id,
                        FileName = imageEntity.FileName
                    }
                );
            }

            return images;
        }



        public async Task<ProfileImage> AddProfileImage(Guid userId, DownloadFileRequestModel fileRequest)
        {
            return _coreUserRepository.AddProfileImage(userId, fileRequest);
        }

        public async Task<bool> RemoveProfileImage(Guid userId, Guid imageId)
        {
            _coreUserRepository.RemoveProfileImage(userId, imageId);

            return true;
        }
    }
}
