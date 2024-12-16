namespace Vanilla.Aspire.Web
{
    public class UsersApiClient(HttpClient httpClient)
    {
     /*   public async Task<Users[]> GetUsersAsync(int maxItems = 10, CancellationToken cancellationToken = default)
        {
            List<Users>? users = null;

            await foreach (var user in httpClient.GetFromJsonAsAsyncEnumerable<Users>("/users", cancellationToken))
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
        }*/
    }
}
