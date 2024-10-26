using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
