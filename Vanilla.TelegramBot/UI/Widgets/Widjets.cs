using System.Resources;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Vanilla.TelegramBot.Models;
using Vanilla.TelegramBot.Pages;
using Vanilla.TelegramBot.Pages.UpdateUser.Models;
using Vanilla_App.Services.Bonus;

namespace Vanilla.TelegramBot.UI.Widgets
{
    public static class Widjets
    {
        //public static string AboutUser(long chatId, TelegramBotClient botClient, ResourceManager resourceManager, UserModel userModel, ReplyMarkup? replyMarkup = null)
        public static string AboutUser(ResourceManager resourceManager, UserActionContextModel updateUserModel)
        {
            var InitMessage = "<b>{0}</b>\n\n{1}\n\n{2}\n\nЗнайти мене можеш тут:\n\n{3}\n\n";

            var links = new List<string>();
            if (updateUserModel.Links is not null) links.AddRange(updateUserModel.Links);
            if (updateUserModel.Username is not null) links.Add("@" + updateUserModel.Username);
            var linkStr = String.Join(", ", links);

            var text = string.Format(InitMessage, updateUserModel.Nickname, updateUserModel.About, updateUserModel.IsRadyForOrders == true ? resourceManager.GetString("IAcceptOrders") : "", linkStr);

            return text;

        }

        public static string AboutUser(ResourceManager resourceManager, Models.UserModel userModel)
        {
            var InitMessage = "<b>{0}</b>\n\n{1}\n\n{2}\n\nЗнайти мене можеш тут:\n\n{3}\n\n";

            var links = new List<string>();
            if (userModel.Links is not null) links.AddRange(userModel.Links);
            if (userModel.Username is not null) links.Add("@" + userModel.Username);
            var linkStr = String.Join(", ", links);

            var text = string.Format(InitMessage, userModel.Nickname, userModel.About, userModel.IsRadyForOrders == true ? resourceManager.GetString("IAcceptOrders") : "", linkStr);

            return text;

        }

        public static SendMessageArgs ThisFeatureIsOnlyForRegisteredUsers (long chatId, ResourceManager resourceManager)
        {
            var message = resourceManager.GetString("ThisFeatureIsOnlyForRegisteredUsers");
            var keyboard = Keyboards.GetCreateProfileKeypoard(resourceManager);
            return new SendMessageArgs(chatId, message) {
                ReplyMarkup = keyboard
            };
        }


        public static string ProblemWithExternalServer()
        {
            return "Ой Йой \n\n На жаль нам не вдалось зв'язатись з одними з наших серверів. Це може бути пов'язане з тимчасовим браком електро харчування, або ще з чимось іншим 😝  \r\n\r\nВибачте за незручності, і повторіть спробу пізніше 😴";
        }

        public static SendPhotoArgs BonusInfo(long chatId, UserBonusModel _bonusObject)
        {
            string message = string.Format("{0} \n\n{1}\n\nЗареєстровано: {2}", _bonusObject.Title, _bonusObject.Description, _bonusObject.DateOfRegistration.ToString("dd.MM.yyyy"));
            if (_bonusObject.IsUsed)
            {
                string activateDate = _bonusObject.DateOfUsed?.ToString("dd.MM.yyyy");
                message += string.Format("\nБонус було успішно активовано: {0}", activateDate);
            }

            var args =  new SendPhotoArgs(chatId: chatId, photo: _bonusObject.CoverUrl);
            args.ParseMode = "HTML";
            args.Caption = message;

            return args;
        }

        public static SendPhotoArgs BonusInfo(long chatId, UserBonusModel _bonusObject, UserContextModel userContext)
        {
            var args = new SendPhotoArgs(chatId: chatId, photo: _bonusObject.CoverUrl);


            string message = string.Format("{0} \n\n{1}\n\nЗареєстровано: {2}", _bonusObject.Title, _bonusObject.Description, _bonusObject.DateOfRegistration.ToString("dd.MM.yyyy"));
            if (_bonusObject.IsUsed)
            {
                string activateDate = _bonusObject.DateOfUsed?.ToString("dd.MM.yyyy");
                message += string.Format("\nБонус було успішно активовано: {0}", activateDate);
            }
            else
            {
                args.ReplyMarkup = GenerateBonusInfoKeyboard(userContext, _bonusObject.BonusId);
            }

            args.ParseMode = "HTML";
            args.Caption = message;

            return args;
        }

        private static InlineKeyboardMarkup GenerateBonusInfoKeyboard(UserContextModel userContext, String bonusId)
        {
            string _deliver = " mya~ ";

            var yesBtn = new InlineKeyboardButton(text: userContext.ResourceManager.GetString("Spend"));

            var replyMarkuppp = new InlineKeyboardMarkup
            (
                new InlineKeyboardButton[][]{
                        new InlineKeyboardButton[]{
                            yesBtn,
                         }
                }
            );

            yesBtn.CallbackData = "bonus" + _deliver + bonusId + _deliver + "ok";

            return replyMarkuppp;
        }
    }
}
