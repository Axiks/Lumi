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
                Images = ImageEntityesToUserImages(userEnity.Images),
                IsHasProfile = userEnity.IsHasProfile,
            };

            return userModel;
        }

        private static ImageModel ImageEntityToUserImage(ImagesEntity imageEntity) => new ImageModel
        {
            TgMediaId = imageEntity.TgMediaId,
            TgUrl = imageEntity.TgUrl,
        };

        private static List<ImageModel>? ImageEntityesToUserImages(List<ImagesEntity> imageEntities)
        {
            if(imageEntities is null) return null;

            var images = new List<ImageModel>();
            foreach (var imageEntity in imageEntities)
            {
                images.Add(ImageEntityToUserImage(imageEntity));
            }

            return images;
        }
    }
}
