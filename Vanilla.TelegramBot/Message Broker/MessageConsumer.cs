using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla.Common.Message_Broker;
using Vanilla.TelegramBot.Interfaces;

namespace Vanilla.TelegramBot.Message_Broker
{
    public class MessageConsumer : IConsumer<Message>
    {
        private readonly ILogger<MessageConsumer> _logger;
        public MessageConsumer(ILogger<MessageConsumer> logger)
        {

            _logger = logger;

        }
        public async Task Consume(ConsumeContext<Message> context)
        {
            var message = context.Message.body;

            _logger.LogInformation("Consuming message: " + message);
        }
    }

    public class TgMessageConsumer : IConsumer<TgUserRequest>
    {
        private readonly ILogger<TgUserRequest> _logger;
        private readonly IUserService _userService;
        public TgMessageConsumer(ILogger<TgUserRequest> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }
        public async Task Consume(ConsumeContext<TgUserRequest> context)
        {
            var message = context.Message.UserId;

            _logger.LogInformation("Consuming message: " + message);

            var tgUser = await _userService.GetUser(context.Message.UserId);

            if (tgUser == null)
                throw new InvalidOperationException("Telegram User not found");

            var images = new List<string>();

            foreach (var image in tgUser.Images)
            {
                images.Add(image.TgMediaId);
            }

            await context.RespondAsync<TgUserResponse>(new
            {
                UserId = tgUser.UserId,
                TgId = tgUser.TelegramId,
                Username = tgUser.Username,
                ImagesId = images
            });
        }
    }
}
