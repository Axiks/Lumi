using Vanilla_App.Interfaces;
using Vanilla_App.Models;
using Vanilla_App.Repository;

namespace Vanilla_App.Services
{
    public class BonusService : IBonusService
    {
        private IBonusRepository _repository { get; init; }
        private List<long> _usersWithBonus { get; }
        public BonusService()
        {
            _repository = new BonusRepository();
            //_repository = new TestBonusRepository();
            _usersWithBonus = _repository.GetUsersWithBonus();
        }
        public List<UserBonusModel>? GetUserBonuses(long tgId)
        {
            return _repository.GetUserBonuses(tgId);
        }

        public bool TakeBonus(string bonusId)
        {
            return _repository.TakeBonus(bonusId);
        }

        public UserBonusModel? GetBonus(string bonusId) => _repository.GetBonus(bonusId);
    }
}
