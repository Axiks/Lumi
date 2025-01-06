using MassTransit;
using Vanilla.Common.Message_Broker;
using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Message_Broker
{
    public class AboutUserConsumer : IConsumer<TgUserRequest>
    {
        public async Task Consume(ConsumeContext<TgUserRequest> context)
        {
            var userId = context.Message.UserId;


            var tgUser = new UserModel
            {
                UserId = Guid.NewGuid(),
                Token = "Test",
                TelegramId = 1234567898,
                RegisterInServiceAt = DateTime.UtcNow,
                RegisterInSystemAt = DateTime.UtcNow,
            };
            //var tgUser = await userService.GetUser(userId);
            if (tgUser == null)
                throw new InvalidOperationException("Telegram User not found");

            var images = new List<string>();
            
            foreach (var image in tgUser.Images)
            {
                images.Add(image.TgMediaId);
            }

            await context.RespondAsync<TgUserResponse>(new
            {
                UserId = userId,
                TgId = tgUser.TelegramId,
                Username = tgUser.Username,
                ImagesId = images
            });
        }
    }
}
