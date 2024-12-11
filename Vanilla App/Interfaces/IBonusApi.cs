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
        [Post("/bonuses/{bonus_id}/take?token={token}")]
        Task<ApiResponse<TakeBonusModel>> TakeBonusAsync(string bonus_id, string token);


        [Get("/bonuses/{bonus_id}?token={token}")]
        Task<ApiResponse<UserBonusModel>> GetBonusAsync(string bonus_id, string token);


        [Get("/users/{user_tg_id}/bonuses?token={token}")]
        Task<ApiResponse<List<UserBonusModel>>> GetUserAsync(long user_tg_id, string token);

    }
}
