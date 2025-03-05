using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Web.Models;
using Web.Services;

namespace Web.Manager.Auth
{
    public class AuthManager : AuthenticationStateProvider, IAuthManager
    {
        private IFirebaseAuthService _authService;
        private ILocalStorageService _localStorage;
        private IMemberService _memberService;
        private IGroupService _groupService;
        private IDepartmentService _departmentService;
        private string loggedInUserKey = "LoggedInUser";
        private string departmentKey = "Department";

        public AuthManager(IFirebaseAuthService authService, 
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

        public async Task<User> SignUp(string email, string password, string displayName)
        {
            var serviceResponse = await _authService.SignUpUser(email, password, displayName);
            if (serviceResponse.ErrorCode == null) 
            {
                if (string.IsNullOrEmpty(serviceResponse.localId))
                    throw new NullReferenceException("Uid of user is null after sign up.");
                var user = new User()
                {
                    Id = serviceResponse.localId,
                    IdToken = serviceResponse.idToken
                };
                await InsertLocalUser(user);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                return user;
            }
            throw new Exception();           
        }

        public async Task<User> SignIn(string email, string password)
        {
            var serviceResponse = await _authService.SignInUser(email, password);
            if (serviceResponse.ErrorCode == null)
            {
                if (string.IsNullOrEmpty(serviceResponse.localId))
                    throw new NullReferenceException("Uid of user is null after sign in.");
                var user = new User()
                {
                    Id = serviceResponse.localId,
                    IdToken = serviceResponse.idToken
                };
                await InsertLocalUser(user);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                return user;
            }
            throw new Exception();
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

            if (user == null)
                return authState;

            var getUserDataResponse = await _authService.GetUserData(user.IdToken);
            if (getUserDataResponse.ErrorCode != null)                
                return authState;

            var identity = new ClaimsIdentity("FirebaseAuth");
            var claims = await CreateClaims(user, getUserDataResponse);
            identity.AddClaims(claims);
            authState.User.AddIdentity(identity);
            return authState;
        }

        private async Task<List<Claim>> CreateClaims(User user, IFirebaseAuthService.GetUserDataResponse getUserDataResponse)
        {
            var claims = new List<Claim>();

            if (!getUserDataResponse.users.First().disabled)
                claims.Add(new Claim(IAuthManager.IsActiveClaim, true.ToString(), ClaimValueTypes.Boolean));

            if (getUserDataResponse.users.First().emailVerified)
                claims.Add(new Claim(IAuthManager.EmailVerifiedClaim, true.ToString(), ClaimValueTypes.Boolean));

            var departmentId = await GetLocalDepartmentId();
            if(string.IsNullOrEmpty(departmentId))
                return claims;

            var inDepartment = await _departmentService.IsMemberInDepartment(user.Id, departmentId);
            if (!inDepartment)
                return claims;
            claims.Add(new Claim(ClaimTypes.Role, "InDepartment"));

            var member = await _memberService.GetMember(departmentId, user.Id);
            if(member.IsAdmin)
                claims.Add(new Claim(ClaimTypes.Role, "IsAdmin"));

            return claims;
        }

        public async Task<string> GetLocalDepartmentId()
        {
            return await _localStorage.GetItemAsync<string>(departmentKey);
        }

        public async Task<Department> GetLocalDepartment()
        {
            var departmentId = await GetLocalDepartmentId();
            if (string.IsNullOrEmpty(departmentId))
                return null;

            return await _departmentService.GetById(departmentId);
        }

        public async Task SetLocalDepartmentId(string departmentId)
        {
            await _localStorage.SetItemAsync(departmentKey, departmentId);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task ResetPassword(string email)
        {
            var response = await _authService.ResetPassword(email);
            if (response.ErrorCode != null)
            {
                throw new Exception();
            }
        }

        public async Task SendVerificationMail(string idToken)
        {
            var response = await _authService.SendVerificationMail(idToken);
            if (response.ErrorCode != null)
            {
                throw new Exception();
            }
        }
    }
}
