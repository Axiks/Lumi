using Refit;

namespace Vanilla_App.Services.Bonus.Repository
{
    public interface IBonusApi
    {
        [Post("/bonuses/{bonus_id}/take?token={token}")]
        Task<ApiResponse<TakeBonusModel>> TakeBonusAsync(string bonus_id, string token);


        [Get("/bonuses/{bonus_id}?token={token}")]
        Task<ApiResponse<UserBonusModel>> GetBonusAsync(string bonus_id, string token);


        [Get("/users/{user_tg_id}/bonuses?token={token}")]
        Task<ApiResponse<List<UserBonusModel>>> GetUserAsync(long user_tg_id, string token);

        [Get("/health")]
        Task<ApiResponse<ServerHealthModel>> GetHealthStatus();

    }
}
