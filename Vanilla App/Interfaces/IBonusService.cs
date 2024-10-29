using Vanilla_App.Models;

namespace Vanilla_App.Interfaces
{
    public interface IBonusService
    {
        public List<UserBonusModel>? GetUserBonuses(long tgId);
        public UserBonusModel? GetBonus(long bonusId);
        public void TakeBonus(int bonusId);
    }
}
