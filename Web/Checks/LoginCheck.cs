using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Web.Manager;
using Web.Services;
namespace Web.Checks
{
    public class LoginCheck : ILoginCheck
    {
        private NavigationManager _navigationManager;
        private AuthenticationStateProvider _authStateProvider;
        private IDepartmentService _departmentService;
        private IAuthManager _authManager;

        public LoginCheck(NavigationManager navigationManager, 
            AuthenticationStateProvider authStateProvider, 
            IAuthManager authManager,
            IDepartmentService departmentService) 
        {
            _navigationManager = navigationManager;
            _authStateProvider = authStateProvider;
            _authManager = authManager;
            _departmentService = departmentService;
        }

        public async Task<bool> CheckLogin(bool needDepartment = false, string departmentId = null)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();

            var authenticated = authState?.User?.Identity?.IsAuthenticated == true;
            if (!authenticated)
            {
                NavigateToLogin();
                return false;
            }

            var verified = authState.User.HasClaim(IAuthManager.EmailVerifiedClaim, true.ToString());
            if (!verified)
            {
                NavigateToLogin();
                return false;
            }

            if (needDepartment == false && string.IsNullOrWhiteSpace(departmentId))
                return true;

            var currentUser = await _authManager.GetLocalUser();
            var currentDepartmentId = await _authManager.GetLocalDepartmentId();

            if (needDepartment == true && string.IsNullOrEmpty(currentDepartmentId) ||
                !string.IsNullOrWhiteSpace(departmentId) && departmentId != currentDepartmentId ||
                !string.IsNullOrEmpty(currentDepartmentId) && !(await _departmentService.IsMemberInDepartment(currentUser.Id, currentDepartmentId)))
            {
                NavigateToDepartment();
                return false;
            }

            return true;
        }

        private void NavigateToVerification()
        {
            _navigationManager.NavigateToLogin("/verification");
        }

        private void NavigateToLogin()
        {
            _navigationManager.NavigateToLogin("/login");
        }

        private void NavigateToDepartment()
        {
            _navigationManager.NavigateToLogin("/department");
        }
    }
}

