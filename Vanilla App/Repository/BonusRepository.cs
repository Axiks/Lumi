using Vanilla_App.Interfaces;
using Vanilla_App.Models;
using Refit;

namespace Vanilla_App.Repository
{
    public class BonusRepository : IBonusRepository
    {
        IBonusApi _api;
        public BonusRepository()
        {
            var _api = RestService.For<IBonusApi>("https://tg-users-api.dan.org.ua");
        }
        public UserBonusModel GetBonus(long bonusId)
        {
            throw new NotImplementedException();
        }

        public List<UserBonusModel> GetUserBonuses(long tgId)
        {
            throw new NotImplementedException();
        }

        public List<long> GetUsersWithBonus()
        {
            throw new NotImplementedException();
        }

        public void TakeBonus(int bonusId)
        {
            throw new NotImplementedException();
        }
    }
}
