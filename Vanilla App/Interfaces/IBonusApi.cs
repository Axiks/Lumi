using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Refit;
using Vanilla_App.Models;

namespace Vanilla_App.Interfaces
{
    public interface IBonusApi
    {
        [Post("/bonuses/{bonus_id}/take")]
        Task<bool> TakeBonusAsync();


        [Get("/bonuses/{bonus_id}")]
        Task<List<UserBonusModel>> GetBonusAsync(int bonus_id);


        [Get("/users/{user_tg_id}/bonuses")]
        Task<List<UserBonusModel>> GetUserAsync(int user_tg_id);

    }
}
