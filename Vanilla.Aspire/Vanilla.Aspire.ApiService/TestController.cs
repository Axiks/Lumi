using MassTransit;
using Vanilla.Common.Message_Broker;
using Microsoft.AspNetCore.Mvc;
using Vanilla_App.Services.Users;
using Vanilla_App.Services.Projects;


namespace Vanilla.Aspire.ApiService
{
    [ApiController]
    [Route("api/test")]
    public class HomeController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger,
            IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }


        [HttpGet]
        public async Task<ActionResult> PublishEndPoint()
        {
            Message message = new Message {
                Id = 1,
                body = "I`m work!",
                CreatedAt = DateTime.UtcNow,
            };
            await _publishEndpoint.Publish(message);

            return Ok("Oki!");
        }
    }


    [ApiController]
    [Route("users")]
    public class TestController(IRequestClient<TgUserRequest> brokerClient, IUserService userService, IProjectService projectService, ILogger<HomeController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> GetAll(Guid userId)
        {
            var users = await userService.GetUsersAsync();

            return Ok(users);
        }

        [HttpGet("{userId:guid}")]
        public async Task<ActionResult> Get(Guid userId)
        {
            var tgUser = await brokerClient.GetResponse<TgUserResponse>(new TgUserRequest { UserId = userId });
            var correUser = await userService.GetUserOrDefaultAsync(userId);

            var user = new Models.UserModel
            {
                Id = correUser.Id,
                Nickname = correUser.Nickname,
                About = correUser.About,
                IsRadyForOrders = correUser.IsRadyForOrders,
                Links = correUser.Links,
                ProfileImages = correUser.ProfileImages,
                TelegramData = tgUser.Message
            };

            //var json = JsonSerializer.Serialize(user);

            return Ok(user);
        }

        [HttpGet("{userId:guid}/projects")]
        public async Task<ActionResult> GetProjects(Guid userId)
        {
            var allProjects = await projectService.ProjectGetAllAsync(); // fix
            var userProjects = allProjects.Where(x => x.OwnerId == userId);

            return Ok(userProjects);
        }
    }



}
