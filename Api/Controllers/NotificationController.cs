using Api.Manager;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private IEventManager _eventManager;

        public NotificationController(IEventManager gameManager)
        {
            _eventManager = gameManager;
        }

        [HttpPost]
        public Task SendHelperNotifications()
        {
            return _eventManager.SendHelperNotifications();
        }
    }
}
