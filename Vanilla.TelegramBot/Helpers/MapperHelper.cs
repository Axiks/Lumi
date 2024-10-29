using Vanilla.TelegramBot.Entityes;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Helpers
{
    public static class MapperHelper
    {
        public static UserCreateResponseModel UserEntityToUserCreateResponseModel(UserEntity userEnity)
        {
            var userModel = new UserCreateResponseModel
            {
                UserId = userEnity.UserId,
                TelegramId = userEnity.TelegramId,
                Username = userEnity.Username,
                FirstName = userEnity.FirstName,
                LastName = userEnity.LastName,
                CreatedAt = userEnity.CreatedAt,
            };

            return userModel;
        }
    }
}
