namespace Vanilla_App.Services.Bonus.Repository
{
    internal interface IBonusRepository
    {
        bool IsOnline();
        List<long> GetUsersWithBonus();
        UserBonusModel GetBonus(string bonusId);
        List<UserBonusModel> GetUserBonuses(long tgId);
        bool TakeBonus(string bonusId);
    }
}
