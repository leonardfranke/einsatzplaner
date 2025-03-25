using Api.Manager;
using DTO;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserManager _userManager;

        public UserController(IUserManager userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("SignIn")]
        public Task<TokenDTO> SignIn([FromBody] AuthenticationDTO authDTO)
        {
            return _userManager.SignInUser(authDTO.Email, authDTO.Password);
        }

        [HttpPost("SignUp")]
        public Task<TokenDTO> SignUp([FromBody] AuthenticationDTO authDTO)
        {
            return _userManager.SignUpUser(authDTO.Email, authDTO.Password, authDTO.DisplayName);
        }

        [HttpGet("GetUserData/{userId}")]
        public Task<UserDTO> GetUserData([FromRoute] string userId)
        {
            return _userManager.GetUserData(userId);
        }

        [HttpGet("ResetPassword")]
        public Task ResetPassword([FromQuery] string email)
        {
            return _userManager.ResetPassword(email);
        }

        [HttpGet("SendVerificationMail")]
        public Task SendVerificationMail([FromQuery] string idtoken)
        {
            return _userManager.SendVerificationMail(idtoken);
        }
    }
}
