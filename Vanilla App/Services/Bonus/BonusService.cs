using Microsoft.Extensions.Configuration;
using Vanilla_App.Services.Bonus.Repository;

namespace Vanilla_App.Services.Bonus
{
    public class BonusService : IBonusService
    {
        private IBonusRepository _repository { get; init; }
        private List<long> _usersWithBonus { get; }
        public BonusService(IConfiguration configuration)
        {
            var provisionBonusApiUrl = configuration.GetValue<string>("provisionBonusApiUrl");
            var provisionBonusApiAccessToken = configuration.GetValue<string>("provisionBonusApiAccessToken");

            _repository = new BonusRepository(provisionBonusApiUrl, provisionBonusApiAccessToken);// temp
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
