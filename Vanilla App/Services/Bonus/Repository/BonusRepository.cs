using Refit;
using Vanilla.Common;
using Vanilla_App.Services.Bonus;

namespace Vanilla_App.Services.Bonus.Repository
{
    public class BonusRepository : IBonusRepository
    {
        readonly IBonusApi _api;
        readonly string Token;
        public BonusRepository()
        {
            /*
                        var setting = new RefitSettings
                        {
                        };


                        var refit = RestService.CreateHttpClient(", setting);
                        refit.

                        _api = RestService.For<IBonusApi>(refit);*/

            ConfigurationMeneger confManager = new ConfigurationMeneger();
            var _settings = confManager.Settings.ProvisionBonusApiConfiguration;
            Token = _settings.Token;

            try
            {
                _api = RestService.For<IBonusApi>(_settings.Url);
            }
            catch (Exception e)
            {
                var x = e.Message;
            }
        }
        public UserBonusModel GetBonus(string bonusId)
        {
            ApiResponse<UserBonusModel> response;

            try
            {
                response = _api.GetBonusAsync(bonusId, Token).Result;
            }
            catch (Exception e)
            {
                throw new HttpRequestException($"Server offline");
            }

            if (response.IsSuccessful == false)
            {
                throw new HttpRequestException($"Failed to fetch bonuses. Status code: {response.StatusCode}");
            }

            return response.Content;
        }

        public List<UserBonusModel> GetUserBonuses(long tgId)
        {
            ApiResponse<List<UserBonusModel>> response;

            try
            {
                response = _api.GetUserAsync(tgId, Token).Result;
            }
            catch (Exception e)
            {
                throw new HttpRequestException($"Server offline");
            }

            if (response.IsSuccessful == false)
            {
                throw new HttpRequestException($"Failed to fetch bonuses. Status code: {response.StatusCode}");
            }

            return response.Content;
        }

        public List<long> GetUsersWithBonus()
        {
            return new List<long> { };
        }

        public bool TakeBonus(string bonusId)
        {
            ApiResponse<TakeBonusModel> response;

            try
            {
                response = _api.TakeBonusAsync(bonusId, Token).Result;
            }
            catch (Exception e)
            {
                throw new HttpRequestException($"Server offline");
            }

            if (response.IsSuccessful == false)
            {
                throw new HttpRequestException($"Failed to fetch bonuses. Status code: {response.StatusCode}");
            }


            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}
