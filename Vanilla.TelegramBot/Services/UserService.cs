using Vanilla.OAuth.Services;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla_App.Module;

namespace Vanilla.TelegramBot.Services
{
    public class UserService : IUserService
    {
        private AuthService _authService;
        private IUserRepository _userRepository;
        private Vanilla_App.Services.UserService _coreUserService;
        public UserService(AuthService authService, IUserRepository userRepository, Vanilla_App.Services.UserService userService)
        {
            _authService = authService;
            _userRepository = userRepository;
            _coreUserService = userService;
        }

        public async Task<List<Models.UserModel>> FindByUsernameAsync(string username)
        {
            var users = await _userRepository.GetUsersAsync(username);

            var response = new List<Models.UserModel>();
            foreach (var localUser in users)
            {
                var coreUser = await _coreUserService.GetUserOrDefaultAsync(localUser.UserId);
                if (coreUser is null) throw new ArgumentException("User don`t exist in core service");

                var userModel =  EntityesToObjectMapperHelper(localUser, coreUser);
                response.Add(userModel);
            }
            return response;
        }

        public async Task<List<Models.UserModel>> GetUsersAsync()
        {
            var users = await _userRepository.GetUsersAsync();

            var response = new List<Models.UserModel>();
            foreach (var localUser in users)
            {
                var coreUser = await _coreUserService.GetUserOrDefaultAsync(localUser.UserId);
                if (coreUser is null) throw new ArgumentException("User don`t exist in core service");

                var userModel = EntityesToObjectMapperHelper(localUser, coreUser);
                response.Add(userModel);
            }
            return response;
        }

        public async Task<Models.UserModel> GetUserAsync(Guid userId)
        {
            var localUser = await _userRepository.GetUserOrDefaultAsync(userId);
            if (localUser is null) throw new ArgumentException("User don`t exist in tg service");

            var coreUser = await _coreUserService.GetUserOrDefaultAsync(userId);
            if (coreUser is null) throw new ArgumentException("User don`t exist in core service");

            _authService.GenerateToken(coreUser.Id);

            return EntityesToObjectMapperHelper(localUser, coreUser);
        }

        // Gegister user in 2 diferent system
        public async Task<Models.UserModel> RegisterUser(UserRegisterModel userRequest)
        {
            var coreUser = await _coreUserService.CreateUserAsync(new Vanilla_App.Services.Users.UserCreateRequestModel
            {
                Nickname = userRequest.Nickname,
                About = userRequest.About,
                IsRadyForOrders = userRequest.IsRadyForOrders,
                Links = userRequest.Links,
            });

            var localUser = await _userRepository.AddUserAsync(new Models.UserCreateRequestModel
            {
                UserId = coreUser.Id,
                TelegramId = userRequest.TelegramId,
                Username = userRequest.Username,
                FirstName = userRequest.FirstName,
                LastName = userRequest.LastName,
                LanguageCode = userRequest.LanguageCode,
            });

            return EntityesToObjectMapperHelper(localUser, coreUser);
        }

        public async Task<Models.UserModel> SignInUserAsync(long telegramId)
        {
            var localUser = await _userRepository.GetUserAsync(telegramId);
            if (localUser is null) throw new ArgumentException("User don`t exist in tg service");

            var coreUser = await _coreUserService.GetUserOrDefaultAsync(localUser.UserId);
            if (coreUser is null) throw new ArgumentException("User don`t exist in core service");

            _authService.GenerateToken(coreUser.Id);

            return EntityesToObjectMapperHelper(localUser, coreUser);
        }

        public async Task<Models.UserModel> UpdateUserAsync(long tgUserId, Models.UserUpdateRequestModel user)
        {
            var localUser = await _userRepository.GetUserAsync(tgUserId);
            if (localUser is null) throw new ArgumentException("User don`t exist in tg service");

            var coreUser = await _coreUserService.GetUserOrDefaultAsync(localUser.UserId);
            if (coreUser is null) throw new ArgumentException("User don`t exist in core service");

            var upcoreUser = await _coreUserService.UpdateUserAsync(localUser.UserId, new Vanilla_App.Services.Users.UserUpdateRequestModel
            {
                Nickname = user.Nickname ?? coreUser.Nickname,
                About = user.About ?? coreUser.About,
                Links = user.Links ?? coreUser.Links,
                IsRadyForOrders = user.IsRadyForOrders ?? coreUser.IsRadyForOrders,
            });

            //if (user.Images is not null && user.Images.Count() > 0) ImageHelper.DownloadProfileImages(user.Images);
            if (user.Images is not null && user.Images.Count() > 0)
            {
                // Remove old images
                if(coreUser.ProfileImages.Count() > 0)
                {
                    foreach(var image in coreUser.ProfileImages)
                    {
                        await _coreUserService.RemoveProfileImageAsync(coreUser.Id, image.Id);
                    }
                }


                foreach ( var image in user.Images)
                {
                    var fileRequest = new DownloadFileRequestModel
                    {
                        FileName = image.TgMediaId,
                        DownloadURL = image.DownloadPath
                    };

                    var coreImage = await _coreUserService.AddProfileImageAsync(coreUser.Id, fileRequest);
                    image.CoreId = coreImage.Id;
                }
            }

            var uplocalUser = await _userRepository.UpdateUserAsync(new Models.UserCreateRequestModel
            {
                TelegramId = localUser.TelegramId,
                UserId = localUser.UserId,
                Username = localUser.Username,
                FirstName = localUser.FirstName,
                LastName = localUser.LastName,
                LanguageCode = localUser.LanguageCode,
                Images = user.Images,
                IsHasProfile = user.IsHasProfile,
            });

            return EntityesToObjectMapperHelper(uplocalUser, upcoreUser);
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var localUser = await _userRepository.GetUserOrDefaultAsync(userId);
            if (localUser is null) throw new ArgumentException("User don`t exist in tg service");

            var coreUser = await _coreUserService.GetUserOrDefaultAsync(userId);
            if (coreUser is null) throw new ArgumentException("User don`t exist in core service");


            _userRepository.DeleteUserAsync(userId);
            await _coreUserService.DeleteUserAsync(userId);
        }


        /*       bool IsUserUploadNewProfileImages(List<ImageModel>? currentImages, List<ImageModel>? newImages) {
                   if (currentImages is null && newImages is null) return false;

                   if (currentImages is null != newImages is null) return true;
                   if(currentImages.Count() != newImages.Count()) return true;
                   foreach (var image in newImages) {
                       if(currentImages.Exists(x => x.TgMediaId == image.TgMediaId) is false) return true;
                   }
                   return false;
               }*/

        private Models.UserModel EntityesToObjectMapperHelper(UserCreateResponseModel localUser, Vanilla_App.Services.Users.UserModel coreUser) => new Models.UserModel
        {
                UserId = coreUser.Id,
                Token = _authService.GenerateToken(coreUser.Id),
                TelegramId = localUser.TelegramId,
                Username = localUser.Username,
                Nickname = coreUser.Nickname,
                FirstName = localUser.FirstName,
                LastName = localUser.LastName,
                About = coreUser.About,
                Links = coreUser.Links,
                IsRadyForOrders = coreUser.IsRadyForOrders,
                Images = localUser.Images,
                RegisterInServiceAt = localUser.CreatedAt,
                RegisterInSystemAt = localUser.CreatedAt,
                IsHasProfile = localUser.IsHasProfile
        };
    }
}
