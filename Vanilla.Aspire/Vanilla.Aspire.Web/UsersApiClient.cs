namespace Vanilla.Aspire.Web
{
    public class UsersApiClient(HttpClient httpClient)
    {
        public async Task<Vanilla_App.Services.Users.UserModel[]> GetUsersAsync(int maxItems = 10, CancellationToken cancellationToken = default)
        {
            List<Vanilla_App.Services.Users.UserModel>? users = null;

            await foreach (var user in httpClient.GetFromJsonAsAsyncEnumerable<Vanilla_App.Services.Users.UserModel>("/users", cancellationToken))
            {
                if (users?.Count >= maxItems)
                {
                    break;
                }
                if (user is not null)
                {
                    users ??= [];
                    users.Add(user);
                }
            }

            return users?.ToArray() ?? [];
        }

        public async Task<Vanilla_App.Services.Users.UserModel?> GetUserByIdAsync(Guid userId, int maxItems = 10, CancellationToken cancellationToken = default)
        {
            return await httpClient.GetFromJsonAsync<Vanilla_App.Services.Users.UserModel>(String.Format("/users/{0}", userId.ToString()), cancellationToken);
        }
    }
}
