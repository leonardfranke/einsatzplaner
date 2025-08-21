using Blazored.LocalStorage;
using Blazored.SessionStorage;
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
        private ISessionStorageService _sessionStorage;
        private IMemberService _memberService;
        private IGroupService _groupService;
        private IDepartmentService _departmentService;
        private string loggedInUserKey = "LoggedInUser";
        private string departmentKey = "Department";

        public AuthManager(IAuthService authService, 
            ILocalStorageService localStorage,
            ISessionStorageService sessionStorage,
            IGroupService groupService, 
            IDepartmentService departmentService,
            IMemberService memberService)
        {
            _authService = authService;
            _localStorage = localStorage;
            _groupService = groupService;
            _departmentService = departmentService;
            _memberService = memberService;
            _sessionStorage = sessionStorage;
        }

        public async Task<User> Authenticate(string email, string password, string displayName, bool isSignUp)
        {
            var tokenDTOTask = isSignUp ? _authService.SignUpUser(email, password, displayName) : _authService.SignInUser(email, password);
            var tokenDTO = await tokenDTOTask;

            if(tokenDTO.Error != null)
            {
                if (tokenDTO.Error == TokenDTO.AuthError.InvalidPassword)
                    throw new IAuthManager.AuthException() { Error = IAuthManager.AuthException.AuthError.InvalidPassword };
                else if (tokenDTO.Error == TokenDTO.AuthError.EmailAlreadyExists)
                    throw new IAuthManager.AuthException() { Error = IAuthManager.AuthException.AuthError.EmailAlreadyExists };
                else if (tokenDTO.Error == TokenDTO.AuthError.EmailNotFound)
                    throw new IAuthManager.AuthException() { Error = IAuthManager.AuthException.AuthError.EmailNotFound };
                else if (tokenDTO.Error == TokenDTO.AuthError.UserDisabled)
                    throw new IAuthManager.AuthException() { Error = IAuthManager.AuthException.AuthError.UserDisabled };
                else //if (tokenDTO.Error == TokenDTO.AuthError.Unknown)
                    throw new IAuthManager.AuthException() { Error = IAuthManager.AuthException.AuthError.Unknown };
            }

            var user = new User()
            {
                Id = tokenDTO.Uid,
                IdToken = tokenDTO.IdToken
            };
            await InsertLocalUser(user);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return user;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = await GetLocalUser();
            return await CreateAuthStateFromUser(user);
        }

        public async Task RemoveLocalUser()
        {
            await _localStorage.RemoveItemAsync(loggedInUserKey);
        }

        private async Task InsertLocalUser(User user)
        {
            await _localStorage.SetItemAsync(loggedInUserKey, user);
        }

        public async Task<User?> GetLocalUser()
        {
            return await _localStorage.GetItemAsync<User>(loggedInUserKey);
        }  

        private async Task<AuthenticationState> CreateAuthStateFromUser(User? user)
        {
            var authState = new AuthenticationState(new ClaimsPrincipal());

            if (string.IsNullOrEmpty(user?.Id))
                return authState;

            try
            {
                var userDTO = await _authService.GetUserData(user.Id);
                if(userDTO == null)
                    return authState;
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

            claims.Add(new Claim(ClaimTypes.Name, userDTO.Name));

            if (!userDTO.IsDisabled)
                claims.Add(new Claim(IAuthManager.IsActiveClaim, true.ToString(), ClaimValueTypes.Boolean));

            if (userDTO.IsEmailVerified)
                claims.Add(new Claim(IAuthManager.EmailVerifiedClaim, true.ToString(), ClaimValueTypes.Boolean));

            var departmentId = await GetCurrentDepartment();
            if(!string.IsNullOrEmpty(departmentId))
            {
                var member = await _memberService.GetMember(departmentId, user.Id);
                if (member != null && member.IsAdmin)
                    claims.Add(new Claim(ClaimTypes.Role, IAuthManager.AdminClaim));
            }

            return claims;
        }

        public async Task SetCurrentDepartment(string departmentId)
        {
            if(await GetCurrentDepartment() != departmentId)
            {
                await _sessionStorage.SetItemAsync(departmentKey, departmentId);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
        }

        public async Task<string> GetCurrentDepartment()
        {
            return await _sessionStorage.GetItemAsync<string>(departmentKey);
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
