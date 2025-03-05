using Microsoft.AspNetCore.Components;
using Web.Manager;

namespace Web.Pages
{
    public class VerificationModalBase : ComponentBase
    {
        [Inject]
        private IAuthManager _authManager { get; set; }

        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        public async Task SendVerificationMail()
        {
            var user = await _authManager.GetLocalUser();
            if (user != null)
                await _authManager.SendVerificationMail(user.IdToken);
            await CloseModalFunc();
        }
    }
}
