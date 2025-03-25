using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Web.Manager;
using Web.Models;
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

        public async Task<bool> CheckLogin(Department department = null)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();

            var authenticated = authState?.User?.Identity?.IsAuthenticated == true;
            if (!authenticated)
            {
                NavigateToLogin(department);
                return false;
            }

            var verified = authState.User.HasClaim(IAuthManager.EmailVerifiedClaim, true.ToString());
            if (!verified)
            {
                NavigateToLogin(department);
                return false;
            }

            if (department == null)
                return true;

            var currentUser = await _authManager.GetLocalUser();
            if (await _departmentService.IsMemberInDepartment(currentUser.Id, department.Id) == false)
            {
                NavigateToMembership(department);
                return false;
            }

            return true;
        }

        private void NavigateToLogin(Department department)
        {
            if(department == null)
                _navigationManager.NavigateToLogin("./login");
            else
                _navigationManager.NavigateToLogin($"./{department.URL}/login");
        }

        private void NavigateToMembership(Department department)
        {
            _navigationManager.NavigateToLogin($"./{department.URL}/membership");
        }
    }
}

