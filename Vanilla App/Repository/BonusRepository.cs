using Vanilla_App.Interfaces;
using Vanilla_App.Models;

namespace Vanilla_App.Repository
{
    public class BonusRepository : IBonusRepository
    {
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
