using Microsoft.AspNetCore.Components;
using Web.Manager;

namespace Web.Layout
{
    public class MainLayoutBase : LayoutComponentBase
    {
        [Inject]
        private NavigationManager _navigationManager { get; set; }

        [Inject]
        private IAuthManager _authManager { get; set; }

        public async Task Logout()
        {
            await _authManager.RemoveLocalUser();
            _navigationManager.Refresh();
        }

        public async Task NavigateToLogin()
        {
            _navigationManager.NavigateTo("./login");
        }

        public void NavigateToDepartment()
        {
            _navigationManager.NavigateTo("./department");
        }
    }
}
