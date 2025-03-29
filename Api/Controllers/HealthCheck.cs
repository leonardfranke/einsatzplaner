using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheck : Controller
    {
        [HttpGet]
        public Task Get()
        {
            return Task.CompletedTask;
        }
    }
}
