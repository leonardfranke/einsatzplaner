using System.Security.Claims;
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

        public async Task<bool> CheckLogin(string departmentUrl, Department department = null, bool requiresAdminRole = false)
        {
            if (department == null && requiresAdminRole)
                throw new ArgumentException("Requires admin role but no department passed", nameof(requiresAdminRole));

            var authState = await _authStateProvider.GetAuthenticationStateAsync();

            var authenticated = authState?.User?.Identity?.IsAuthenticated == true;
            if (!authenticated)
            {
                NavigateToLogin(departmentUrl);
                return false;
            }

            var verified = authState.User.HasClaim(IAuthManager.EmailVerifiedClaim, true.ToString());
            if (!verified)
            {
                NavigateToLogin(departmentUrl);
                return false;
            }

            if (department == null)
                return true;

            var currentUser = await _authManager.GetLocalUser();
            if (await _departmentService.IsMemberInDepartment(currentUser.Id, department.Id) == false)
            {
                NavigateToMembership(departmentUrl);
                return false;
            }

            if(requiresAdminRole && !authState.User.HasClaim(ClaimTypes.Role, IAuthManager.AdminClaim))
            {
                NavigateToLogin(departmentUrl);
                return false;
            }  

            return true;
        }

        private void NavigateToLogin(string departmentUrl)
        {
            _navigationManager.NavigateToLogin($"./{departmentUrl}/login");
        }

        private void NavigateToMembership(string departmentUrl)
        {
            _navigationManager.NavigateToLogin($"./{departmentUrl}/membership");
        }
    }
}

