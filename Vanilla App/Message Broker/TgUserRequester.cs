using MassTransit;
using Vanilla.Common.Message_Broker;

namespace Vanilla_App.Message_Broker
{
    internal class TgUserRequester
    {
        private readonly IRequestClient<TgUserRequest> _requestClient;

        public TgUserRequester(IRequestClient<TgUserRequest> requestClient)
        {
            _requestClient = requestClient;
        }

        public async Task<TgUserResponse> RequestDataAsync(Guid UserId)
        {
            var response = await _requestClient.GetResponse<TgUserResponse>(new TgUserRequest { UserId = UserId });
            return response.Message;
        }
    }
}
