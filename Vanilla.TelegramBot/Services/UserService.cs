using System.Drawing;
using System.IO;
using Telegram.BotAPI.AvailableTypes;
using Vanilla.OAuth.Models;
using Vanilla.OAuth.Services;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using static System.Net.Mime.MediaTypeNames;
using File = System.IO.File;

namespace Vanilla.TelegramBot.Services
{
    public class UserService : IUserService
    {
        private Vanilla.OAuth.Services.UserRepository _oauthUserService;
        private AuthService _authService;
        private IUserRepository _userRepository;
        private Vanilla_App.Services.UserService _coreUserService;
        public UserService(AuthService authService, IUserRepository userRepository, Vanilla.OAuth.Services.UserRepository oauthUserService, Vanilla_App.Services.UserService userService)
        {
            _oauthUserService = oauthUserService;
            _authService = authService;
            _userRepository = userRepository;
            _coreUserService = userService;
        }

        public async Task<List<UserModel>> FindByUsername(string username)
        {
            var users = await _userRepository.GetUsersAsync(username);

            var response = new List<UserModel>();
            foreach (var localUser in users)
            {
                var oauthUser = await _oauthUserService.GetUserAsync(localUser.UserId);
                if (oauthUser is null) throw new Exception("User don`t exist in oauth service");

                var coreUser = _coreUserService.GetUser(localUser.UserId);
                if (coreUser is null) throw new Exception("User don`t exist in core service");


                var userModel =  EntityesToObjectMapperHelper(localUser, coreUser, oauthUser);
                response.Add(userModel);
            }
            return response;
        }

        public async Task<List<UserModel>> GetUsers()
        {
            var users = await _userRepository.GetUsersAsync();

            var response = new List<UserModel>();
            foreach (var localUser in users)
            {
                var oauthUser = await _oauthUserService.GetUserAsync(localUser.UserId);
                if (oauthUser is null) throw new Exception("User don`t exist in oauth service");

                var coreUser = _coreUserService.GetUser(localUser.UserId);
                if (coreUser is null) throw new Exception("User don`t exist in core service");


                var userModel = EntityesToObjectMapperHelper(localUser, coreUser, oauthUser);
                response.Add(userModel);
            }
            return response;
        }

        public async Task<UserModel> GetUser(Guid userId)
        {
            var localUser = await _userRepository.GetUserAsync(userId);
            if (localUser is null) throw new Exception("User don`t exist in tg service");

            var oauthUser = await _oauthUserService.GetUserAsync(userId);
            if (oauthUser is null) throw new Exception("User don`t exist in oauth service");

            var coreUser = _coreUserService.GetUser(userId);
            if (coreUser is null) throw new Exception("User don`t exist in core service");

            _authService.GenerateToken(oauthUser);

            return EntityesToObjectMapperHelper(localUser, coreUser, oauthUser);
        }

        // Gegister user in 2 diferent system
        public async Task<UserModel> RegisterUser(UserRegisterModel userRequest)
        {
            var oauthUser = await _oauthUserService.CreateUserAsync(new OAuth.Models.UserCreateRequestModel
            {
                NickName = userRequest.Username,
            });

            var coreUser = _coreUserService.CreateUser(new Vanilla_App.Models.UserCreateRequestModel
            {
                UserId = oauthUser.Id,
                About = userRequest.About,
                IsRadyForOrders = userRequest.IsRadyForOrders,
                Links = userRequest.Links,
            });

            var localUser = await _userRepository.AddUserAsync(new Models.UserCreateRequestModel
            {
                UserId = oauthUser.Id,
                TelegramId = userRequest.TelegramId,
                Username = userRequest.Username,
                FirstName = userRequest.FirstName,
                LastName = userRequest.LastName,
                LanguageCode = userRequest.LanguageCode,
            });

            return EntityesToObjectMapperHelper(localUser, coreUser, oauthUser);
        }

        public async Task<UserModel> SignInUser(long telegramId)
        {
            var localUser = await _userRepository.GetUserAsync(telegramId);
            if (localUser is null) throw new Exception("User don`t exist in tg service");

            var oauthUser = await _oauthUserService.GetUserAsync(localUser.UserId);
            if (oauthUser is null) throw new Exception("User don`t exist in oauth service");

            var coreUser = _coreUserService.GetUser(localUser.UserId);
            if (coreUser is null) throw new Exception("User don`t exist in core service");

            _authService.GenerateToken(oauthUser);

            return EntityesToObjectMapperHelper(localUser, coreUser, oauthUser);
        }

        public async Task<UserModel> UpdateUser(long tgUserId, Models.UserUpdateRequestModel user)
        {
            var localUser = await _userRepository.GetUserAsync(tgUserId);
            if (localUser is null) throw new Exception("User don`t exist in tg service");

            var oauthUser = await _oauthUserService.GetUserAsync(localUser.UserId);
            if (oauthUser is null) throw new Exception("User don`t exist in oauth service");

            var coreUser = _coreUserService.GetUser(localUser.UserId);
            if (coreUser is null) throw new Exception("User don`t exist in core service");

            var upauthUser = await _oauthUserService.UpdateUserAsync(localUser.UserId, new OAuth.Models.UserUpdateRequestModel
            {
                NickName = user.Nickname ?? oauthUser.Nickname,
            });

            var upcoreUser = _coreUserService.UpdateUser(localUser.UserId, new Vanilla_App.Models.UserUpdateRequestModel
            {
                About = user.About ?? coreUser.About,
                Links = user.Links ?? coreUser.Links,
                IsRadyForOrders = user.IsRadyForOrders ?? coreUser.IsRadyForOrders,
            });

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

            //if (IsUserUploadNewProfileImages(localUser.Images, user.Images)) DownloadProfileImages(user.Images);
            if (user.Images is not null && user.Images.Count() > 0) DownloadProfileImages(user.Images);

            return EntityesToObjectMapperHelper(uplocalUser, upcoreUser, upauthUser);
        }

        public async Task DeleteUser(Guid userId)
        {
            var localUser = await _userRepository.GetUserAsync(userId);
            if (localUser is null) throw new Exception("User don`t exist in tg service");

            var oauthUser = await _oauthUserService.GetUserAsync(localUser.UserId);
            if (oauthUser is null) throw new Exception("User don`t exist in oauth service");

            var coreUser = _coreUserService.GetUser(localUser.UserId);
            if (coreUser is null) throw new Exception("User don`t exist in core service");

            _userRepository.RemoveUserAsync(userId);
            _coreUserService.DeleteUser(userId);
            _oauthUserService.DeleteUser(userId);

        }

        async Task DownloadProfileImages(List<ImageModel> images)
        {
            foreach (var image in images)
            {
                Directory.CreateDirectory("storage");

                var imagePath = "storage\\" + image.TgMediaId + ".jpg";
                await DownloadImageAsync(image.DownloadPath, imagePath);
                MakeResizeImage(imagePath);
            }
        }

        async Task DownloadImageAsync(string imageUrl, string fileName)
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(imageUrl);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(fileName, FileMode.Create);
            await stream.CopyToAsync(fileStream);
        }

        async Task MakeResizeImage(string path)
        {
            var original = System.Drawing.Image.FromFile(path);
            var thumbnail = await ScaleImage(original, 256);

            var iamgePathWichoutJpg = path.Split(".jpg").First();
            var thumbnailPath = iamgePathWichoutJpg + "_thumbnail.jpg";
            thumbnail.Save(thumbnailPath);
        }

        async Task<System.Drawing.Image> ScaleImage(System.Drawing.Image image, int height)
        {
            double ratio = (double)height / image.Height;
            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);
            Bitmap newImage = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            image.Dispose();
            return newImage;
        }


        bool IsUserUploadNewProfileImages(List<ImageModel>? currentImages, List<ImageModel>? newImages) {
            if (currentImages is null && newImages is null) return false;

            if (currentImages is null != newImages is null) return true;
            if(currentImages.Count() != newImages.Count()) return true;
            foreach (var image in newImages) {
                if(currentImages.Exists(x => x.TgMediaId == image.TgMediaId) is false) return true;
            }
            return false;
        }

        private UserModel EntityesToObjectMapperHelper(UserCreateResponseModel localUser, Vanilla_App.Models.UserModel coreUser, BasicUserModel oauthUser) => new UserModel
        {
                UserId = oauthUser.Id,
                Token = _authService.GenerateToken(oauthUser),
                TelegramId = localUser.TelegramId,
                Username = localUser.Username,
                Nickname = oauthUser.Nickname,
                FirstName = localUser.FirstName,
                LastName = localUser.LastName,
                About = coreUser.About,
                Links = coreUser.Links,
                IsRadyForOrders = coreUser.IsRadyForOrders,
                Images = localUser.Images,
                RegisterInServiceAt = localUser.CreatedAt,
                RegisterInSystemAt = oauthUser.CreatedAt,
                IsHasProfile = localUser.IsHasProfile
        };
    }
}
