using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.OAuth.Models;
using Vanilla.OAuth.Services;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Repositories;

namespace Vanilla.TelegramBot.Services
{
    public class UserService : IUserService
    {
        private Vanilla.OAuth.Services.UserRepository _oauthUserService;
        private AuthService _authService;
        private IUserRepository _userRepository;
        public UserService(AuthService authService, IUserRepository userRepository, Vanilla.OAuth.Services.UserRepository oauthUserService)
        {
            _oauthUserService = oauthUserService;
            _authService = authService;
            _userRepository = userRepository;
        }

        public async Task<UserModel> GetUser(Guid userId)
        {
            var localUser = await _userRepository.GetUserAsync(userId);
            if (localUser is null) throw new Exception("User don`t exist in tg service");

            var oauthUser = await _oauthUserService.GetUserAsync(userId);
            if (oauthUser is null) throw new Exception("User don`t exist in oauth service");

            _authService.GenerateToken(oauthUser);

            var userModel = new UserModel
            {
                UserId = oauthUser.Id,
                Token = _authService.GenerateToken(oauthUser),
                TelegramId = localUser.TelegramId,
                Username = localUser.Username,
                FirstName = localUser.FirstName,
                LastName = localUser.LastName,
                RegisterInServiceAt = localUser.CreatedAt,
                RegisterInSystemAt = oauthUser.CreatedAt
            };
            return userModel;
        }

        // Gegister user in 2 diferent system
        public async Task<UserModel> RegisterUser(UserRegisterModel userRequest)
        {
            var oauthUser = await _oauthUserService.CreateUserAsync(new OAuth.Models.UserCreateRequestModel
            {
                Username = userRequest.Username,
            });

            var localUser = await _userRepository.AddUserAsync(new Models.UserCreateRequestModel
            {
                UserId = oauthUser.Id,
                TelegramId = userRequest.TelegramId,
                Username = userRequest.Username,
                FirstName = userRequest.FirstName, 
                LastName = userRequest.LastName
            });

            var userModel = new UserModel
            {
                UserId = oauthUser.Id,
                Token = _authService.GenerateToken(oauthUser),
                TelegramId = localUser.TelegramId,
                Username = localUser.Username,
                FirstName = localUser.FirstName,
                LastName = localUser.LastName,
                RegisterInServiceAt = localUser.CreatedAt,
                RegisterInSystemAt = oauthUser.CreatedAt
            };
            return userModel;
        }

        public async Task<UserModel> SignInUser(long telegramId)
        {
            var localUser = await _userRepository.GetUserAsync(telegramId);
            if (localUser is null) throw new Exception("User don`t exist in tg service");

            var oauthUser = await _oauthUserService.GetUserAsync(localUser.UserId);
            if (oauthUser is null) throw new Exception("User don`t exist in oauth service");

            _authService.GenerateToken(oauthUser);

            var userModel = new UserModel
            {
                UserId = oauthUser.Id,
                Token = _authService.GenerateToken(oauthUser),
                TelegramId = localUser.TelegramId,
                Username = localUser.Username,
                FirstName = localUser.FirstName,
                LastName = localUser.LastName,
                RegisterInServiceAt = localUser.CreatedAt,
                RegisterInSystemAt = oauthUser.CreatedAt
            };
            return userModel;
        }
    }
}
