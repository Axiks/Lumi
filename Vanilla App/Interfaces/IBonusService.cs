using Vanilla_App.Models;

namespace Vanilla_App.Interfaces
{
    public interface IBonusService
    {
        public List<UserBonusModel>? GetUserBonuses(long tgId);
        public UserBonusModel? GetBonus(string bonusId);
        public bool TakeBonus(string bonusId);
    }
}
