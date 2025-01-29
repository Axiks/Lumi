using Vanilla.TelegramBot.Models;

namespace Vanilla.TelegramBot.Services.Bot_Service
{
    public class UserContextMenager
    {
        List<UserContextModel> _usersContext;
        public UserContextMenager()
        {
            _usersContext = new List<UserContextModel>();
        }

        public UserContextModel Get(long tgUserId) => _usersContext.FirstOrDefault(x => x.UpdateUser.TgId == tgUserId);
        public UserContextModel? Get(Guid userId) => _usersContext.FirstOrDefault(x => x.User.UserId == userId);
        public UserContextModel Add(UpdateUserData updateUser)
        {
            var context = new UserContextModel(updateUser);
            _usersContext.Add(context);
            return context;
        }
        public UserContextModel Add(UpdateUserData updateUser, UserModel user)
        {
            var context = new UserContextModel(updateUser, user);
            _usersContext.Add(context);
            return context;
        }

        public bool Remove(Guid userId)
        {
            if (_usersContext.Exists(x => x.User.UserId == userId) is false) return false;

            var context = _usersContext.First(x => x.User.UserId == userId);
            return _usersContext.Remove(context);
        }

        public bool Remove(long tgUserId)
        {
            if(_usersContext.Exists(x => x.UpdateUser.TgId == tgUserId) is false) return false;

            var context = _usersContext.First(x => x.UpdateUser.TgId == tgUserId);
            return _usersContext.Remove(context);
        }

    }
}
