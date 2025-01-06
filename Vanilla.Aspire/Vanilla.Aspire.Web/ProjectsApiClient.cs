namespace Vanilla.Aspire.Web
{
    public class ProjectsApiClient(HttpClient httpClient)
    {
        public async Task<Vanilla_App.Services.Projects.ProjectModel[]> GetUserProjectsAsync(Guid userId, int maxItems = 10, CancellationToken cancellationToken = default)
        {
            List<Vanilla_App.Services.Projects.ProjectModel>? projects = null;

            await foreach (var project in httpClient.GetFromJsonAsAsyncEnumerable<Vanilla_App.Services.Projects.ProjectModel>(String.Format("users/{0}/projects", userId), cancellationToken))
            {
                if (projects?.Count >= maxItems)
                {
                    break;
                }
                if (project is not null)
                {
                    projects ??= [];
                    projects.Add(project);
                }
            }

            return projects?.ToArray() ?? [];
        }

    }
}
