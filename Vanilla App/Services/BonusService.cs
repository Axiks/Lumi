using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla_App.Interfaces;
using Vanilla_App.Models;
using Vanilla_App.Repository;

namespace Vanilla_App.Services
{
    public class BonusService : IBonusService
    {
        private IBonusRepository _repository { get; init; }
        private List<long> _usersWithBonus { get; }
        public BonusService() {
            _repository = new TestBonusRepository();
            _usersWithBonus = _repository.GetUsersWithBonus();
        }
        public List<UserBonusModel>? GetUserBonuses(long tgId)
        {
            //return _usersWithBonus.FirstOrDefault(x => x == tgId.ToString()) != null ? _repository.GetUserBonuses(tgId.ToString()) : null;
            return _repository.GetUserBonuses(tgId);
        }

        public void TakeBonus(int bonusId)
        {
            _repository.TakeBonus(bonusId);
        }

        public UserBonusModel? GetBonus(long bonusId) => _repository.GetBonus(bonusId);
    }
}
