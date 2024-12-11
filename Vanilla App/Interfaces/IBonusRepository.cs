using Vanilla_App.Models;

namespace Vanilla_App.Interfaces
{
    internal interface IBonusRepository
    {
        List<long> GetUsersWithBonus();
        UserBonusModel GetBonus(string bonusId);
        List<UserBonusModel> GetUserBonuses(long tgId);
        bool TakeBonus(string bonusId);
    }
}
