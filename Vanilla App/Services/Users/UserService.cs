using Microsoft.Extensions.Configuration;
using Vanilla.Data.Entities;
using Vanilla_App.Module;
using Vanilla_App.Services.Projects;
using Vanilla_App.Services.Projects.Repository;
using Vanilla_App.Services.Users;
using Vanilla_App.Services.Users.Repository;

namespace Vanilla_App.Services
{
    public class UserService(IUserRepository _coreUserRepository, IProjectRepository _projectRepository, Vanilla.OAuth.Services.UserRepository _oauthUserService, IConfiguration configuration) : IUserService
    {
        public async Task<List<UserModel>> GetUsersAsync()
        {
            var users = new List<UserModel>();

            var entities = _coreUserRepository.GetAll();
            foreach (var entity in entities) {
                try
                {
                    var OAuthUser = await _oauthUserService.GetUserAsync(entity.Id);
                    users.Add(ToUserModelMapper(entity, OAuthUser));
                }
                catch
                {
                    throw new Exception("User don`t found in OAuth service. User ID: " + entity.Id);
                }
            }

            return users;
        }

        public async Task<UserModel> GetUserAsync(Guid userId)
        {
            var OAthUser = await _oauthUserService.GetUserAsync(userId);
            var entity = _coreUserRepository.Get(userId);

            return ToUserModelMapper(entity, OAthUser);
        }

        public async Task<UserModel?> GetUserOrDefaultAsync(Guid userId)
        {
            // oauth
            var OAthUser = await _oauthUserService.GetUserOrDefaultAsync(userId);
            if (OAthUser is null) return null;

            var entity = _coreUserRepository.GetOrDefault(userId);
            if (entity is null) return null;

            return ToUserModelMapper(entity, OAthUser);
        }

        public async Task<UserModel> CreateUserAsync(UserCreateRequestModel create)
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

        public async Task<UserModel> UpdateUserAsync(Guid userId, UserUpdateRequestModel update)
        {
            // oauth
            var OAthUser = await _oauthUserService.GetUserAsync(userId);

            var entity = _coreUserRepository.Update(userId, new CoreUserUpdateRequestModel
            {
                About = update.About,
                Links = update.Links,
                IsRadyForOrders = update.IsRadyForOrders,
            });

            var updatetOAthUser = await _oauthUserService.UpdateUserAsync(userId, new Vanilla.OAuth.Models.UserUpdateRequestModel
            {
                NickName = update.Nickname ?? OAthUser.Nickname
            });

            return ToUserModelMapper(entity, updatetOAthUser);
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            // oauth
            _oauthUserService.DeleteUserAsync(userId);

            //Core
            // delete the all user projects
            //foreach (var project in await _coreUserRepository.GetProjectsAsync(userId))
            //{
            //    _projectRepository.Delete(project.Id);
            //}

            _coreUserRepository.Delete(userId);

            return true;
        }

        public async Task<List<ProjectModel>> UserProjectsGetAsync(UserModel user)
        {
            return await _coreUserRepository.GetProjectsAsync(user.Id);
        }

        private UserModel ToUserModelMapper(UserEntity entity, Vanilla.OAuth.Models.BasicUserModel OAuthUser) => new UserModel
        {
            Id = entity.Id,
            About = entity.About,
            Links = entity.Links,
            IsRadyForOrders = entity.IsRadyForOrders,
            Nickname = OAuthUser.Nickname,
            ProfileImages = ImagesEntityToProfileImagesMapper(entity.ProfileImages),
        };

        private List<ProfileImage> ImagesEntityToProfileImagesMapper(List<ImageEntity>? imagesEntity)
        {
            var domain = configuration.GetValue<string>("cdnDomain");
            var domaintorageName = new Uri(domain).ToString() + "storage/";


            if (imagesEntity is null) return new List<ProfileImage> { };

            var images = new List<ProfileImage>();

            foreach (var imageEntity in imagesEntity)
            {
                images.Add(
                    new ProfileImage
                    {
                        Id = imageEntity.Id,
                        FileName = imageEntity.FileName,
                        FileHref = domaintorageName + imageEntity.FileName
                    }
                );
            }

            return images;
        }



        public async Task<ProfileImage> AddProfileImageAsync(Guid userId, DownloadFileRequestModel fileRequest)
        {
            return _coreUserRepository.AddProfileImage(userId, fileRequest);
        }

        public async Task<bool> RemoveProfileImageAsync(Guid userId, Guid imageId)
        {
            _coreUserRepository.RemoveProfileImage(userId, imageId);

            return true;
        }
    }
}
