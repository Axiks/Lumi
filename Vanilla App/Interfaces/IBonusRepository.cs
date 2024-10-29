using Vanilla_App.Models;

namespace Vanilla_App.Interfaces
{
    internal interface IBonusRepository
    {
        List<long> GetUsersWithBonus();
        UserBonusModel GetBonus(long bonusId);
        List<UserBonusModel> GetUserBonuses(long tgId);
        void TakeBonus(int bonusId);
    }
}
