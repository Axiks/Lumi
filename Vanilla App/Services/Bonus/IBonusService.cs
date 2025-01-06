namespace Vanilla_App.Services.Bonus
{
    public interface IBonusService
    {
        public List<UserBonusModel>? GetUserBonuses(long tgId);
        public UserBonusModel? GetBonus(string bonusId);
        public bool TakeBonus(string bonusId);
    }
}
