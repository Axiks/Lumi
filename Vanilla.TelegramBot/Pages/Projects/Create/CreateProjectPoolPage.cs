using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.GettingUpdates;
using Vanilla.Common.Enums;
using Vanilla.TelegramBot.Abstract;
using Vanilla.TelegramBot.Entityes;
using Vanilla.TelegramBot.Interfaces;
using Vanilla.TelegramBot.Models;
using Vanilla_App.Services.Projects;
using static Vanilla.TelegramBot.Abstract.ActionFrame;

namespace Vanilla.TelegramBot.Pages.Projects.Create
{
    public class CreateProjectPoolPage : BasicPageAbstract, IPage
    {
        BotCreateProjectModel? _projectCreateModel;
        ProjectUpdateRequestModel? _projectUpdateModel;
        TelegramBotClient _botClient;
        UserContextModel _userContext;
        List<SendedMessageModel> _sendedMessages;

        private string _pollIdDevelopmentStatus;

        public CreateProjectPoolPage(TelegramBotClient botClient, UserContextModel userContext, List<SendedMessageModel> sendedMessages, BotCreateProjectModel? projectCreateModel = null, ProjectUpdateRequestModel? projectUpdateModel = null) : base(botClient, userContext, sendedMessages)
        {
            _projectCreateModel = projectCreateModel;
            _projectUpdateModel = projectUpdateModel;
            _botClient = botClient;
            _userContext = userContext;
            _sendedMessages = sendedMessages;
        }

        public override void InitMessage()
        {
            var pollArgs = MessageWidgets.GeneratePull(ChatId, _userContext);
            var messInit = _botClient.SendPoll(pollArgs);

            _pollIdDevelopmentStatus = messInit.Poll.Id;

            AddMessage(messInit.MessageId, DeleteMessageMethodEnum.ClosePage);
        }

        public override void InitActions()
        {
            var poolInputAction = new ActionFrame
            {
                ActionObj = new ActionDelegate(PoolAnswerAction),
                Trigger = () => PoolAnswernputTrigger(),
                Validate = new ValidateDelegate(PoolAnswerValidate)
            };

            AddAction(new List<ActionFrame> {
                poolInputAction
            });
        }

        bool PoolAnswernputTrigger() => Helpers.ValidatorHelpers.PoolAnswerValidate(CurrentUpdate);

        bool PoolAnswerValidate(Telegram.BotAPI.GettingUpdates.Update update) => true;
        void PoolAnswerAction(Telegram.BotAPI.GettingUpdates.Update update)
        {
            var poll = update.PollAnswer;

            var optionIndex = poll.OptionIds.First();
            var statusAsList = Enum.GetValues(typeof(DevelopmentStatusEnum)).Cast<DevelopmentStatusEnum>().ToList();

            var selectedOption = statusAsList[optionIndex];

            if (_projectCreateModel is not null) _projectCreateModel.DevelopmentStatus = selectedOption;
            else if (_projectUpdateModel is not null) _projectUpdateModel.ProjectRequest = selectedOption;

            NextPage();
        }

    }
}
