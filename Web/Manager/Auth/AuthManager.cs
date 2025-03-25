using Blazored.LocalStorage;
using DTO;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Web.Models;
using Web.Services;

namespace Web.Manager
{
    public class AuthManager : AuthenticationStateProvider, IAuthManager
    {
        private IAuthService _authService;
        private ILocalStorageService _localStorage;
        private IMemberService _memberService;
        private IGroupService _groupService;
        private IDepartmentService _departmentService;
        private string loggedInUserKey = "LoggedInUser";
        private string departmentKey = "Department";

        public AuthManager(IAuthService authService, 
            ILocalStorageService localStorage, 
            IGroupService groupService, 
            IDepartmentService departmentService,
            IMemberService memberService)
        {
            _authService = authService;
            _localStorage = localStorage;
            _groupService = groupService;
            _departmentService = departmentService;
            _memberService = memberService;
        }

        public async Task<User> Authenticate(string email, string password, string displayName, bool isSignUp)
        {
            var tokenDTOTask = isSignUp ? _authService.SignUpUser(email, password, displayName) : _authService.SignInUser(email, password);
            var tokenDTO = await tokenDTOTask;

            var user = new User()
            {
                Id = tokenDTO.Uid,
                IdToken = tokenDTO.IdToken
            };
            await InsertLocalUser(user);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return user;
        }

        public async Task SignOut()
        {
            await _localStorage.RemoveItemAsync(loggedInUserKey);
            await SetLocalDepartmentId(null);
        }

        private async Task InsertLocalUser(User user)
        {
            await _localStorage.SetItemAsync(loggedInUserKey, user);
        }

        public async Task<User?> GetLocalUser()
        {
            return await _localStorage.GetItemAsync<User>(loggedInUserKey);
        }  

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = await GetLocalUser();
            return await CreateAuthStateFromUser(user);
        }

        private async Task<AuthenticationState> CreateAuthStateFromUser(User? user)
        {
            var authState = new AuthenticationState(new ClaimsPrincipal());

            if (string.IsNullOrEmpty(user?.Id))
                return authState;

            try
            {
                var userDTO = await _authService.GetUserData(user.Id);
                var identity = new ClaimsIdentity("FirebaseAuth");
                var claims = await CreateClaims(user, userDTO);
                identity.AddClaims(claims);
                authState.User.AddIdentity(identity);
                return authState;
            }
            catch
            {
                return authState;
            }
        }

        private async Task<List<Claim>> CreateClaims(User user, UserDTO userDTO)
        {
            var claims = new List<Claim>();

            if (!userDTO.IsDisabled)
                claims.Add(new Claim(IAuthManager.IsActiveClaim, true.ToString(), ClaimValueTypes.Boolean));

            if (userDTO.IsEmailVerified)
                claims.Add(new Claim(IAuthManager.EmailVerifiedClaim, true.ToString(), ClaimValueTypes.Boolean));

            return claims;
        }

        public async Task SetLocalDepartmentId(string departmentId)
        {
            await _localStorage.SetItemAsync(departmentKey, departmentId);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public Task ResetPassword(string email)
        {
            return _authService.ResetPassword(email);
        }

        public Task SendVerificationMail(string idToken)
        {
            return _authService.SendVerificationMail(idToken);
        }
    }
}
