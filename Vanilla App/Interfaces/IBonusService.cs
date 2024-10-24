using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla_App.Models;

namespace Vanilla_App.Interfaces
{
    internal interface IBonusService
    {
        List<string> GetUsersWithBonus();
        List<UserBonusModel> GetUserBonuses(string tgId);
        void TakeBonus(int bonusId);
    }
}
