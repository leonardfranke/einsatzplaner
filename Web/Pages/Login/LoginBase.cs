using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Web.Manager;
using Web.Navigation;
using Web.Pages.Login;
using Web.Views.ResetPassword;

namespace Web.Pages
{
    public class LoginBase : ComponentBase
    {

        [CascadingParameter]
        public Modal Modal { get; set; }

        [Parameter]
        public string DepartmentUrl { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }
        [Inject]
        private IAuthManager _authManager { get; set; }
        [Inject]
        private AuthenticationStateProvider _authStateProvider { get; set; }
        [Inject]
        private ILogger<LoginBase> _logger { get; set; }

        public EditContext EditContextLogin { get; set; }
        public EditContext EditContextRegister { get; set; }

        [SupplyParameterFromForm]
        public LoginModel LoginData { get; set; }

        [SupplyParameterFromForm]
        public RegisterModel RegisterData { get; set; }
        private ValidationMessageStore _messageStoreLogin;
        private ValidationMessageStore _messageStoreRegister;

        protected override async Task OnInitializedAsync()
        {
            LoginData ??= new();
            EditContextLogin = new(LoginData);
            EditContextLogin.OnValidationRequested += ValidateFormLogin;
            _messageStoreLogin = new(EditContextLogin);
            RegisterData ??= new();
            EditContextRegister = new(RegisterData);
            EditContextRegister.OnValidationRequested += ValidateFormRegister;
            _messageStoreRegister = new(EditContextRegister);

            var authState = await _authStateProvider.GetAuthenticationStateAsync();

            var authenticated = authState?.User?.Identity?.IsAuthenticated == true;
            var verified = authState.User.HasClaim(IAuthManager.EmailVerifiedClaim, true.ToString());
            if (authenticated && !verified)
                await ShowVerificationModal();


            await base.OnInitializedAsync();
        }

        private void ValidateFormLogin(object? sender, ValidationRequestedEventArgs e)
        {
            _messageStoreLogin.Clear();

            if (string.IsNullOrWhiteSpace(LoginData.Email))
                _messageStoreLogin.Add(() => LoginData.Email, "Eine E-Mail Adresse wird benötigt");

            if (string.IsNullOrWhiteSpace(LoginData.Password))
                _messageStoreLogin.Add(() => LoginData.Password, "Ein Password wird benötigt");
        }

        private void ValidateFormRegister(object? sender, ValidationRequestedEventArgs e)
        {
            _messageStoreRegister.Clear();

            if (string.IsNullOrWhiteSpace(RegisterData.FirstName))
                _messageStoreRegister.Add(() => RegisterData.FirstName, "Dein Vorname wird benötigt");

            if (string.IsNullOrWhiteSpace(RegisterData.LastName))
                _messageStoreRegister.Add(() => RegisterData.LastName, "Dein Nachname wird benötigt");

            if (string.IsNullOrWhiteSpace(RegisterData.Email))
                _messageStoreRegister.Add(() => RegisterData.Email, "Eine E-Mail Adresse wird benötigt");

            if (string.IsNullOrWhiteSpace(RegisterData.Password))
                _messageStoreRegister.Add(() => RegisterData.Password, "Ein Password wird benötigt");
        }

        public async Task ShowVerificationModal()
        {
            var closeModalFunc = Modal.HideAsync;
            var parameters = new Dictionary<string, object>
            {
                { nameof(VerificationModal.CloseModalFunc), closeModalFunc }
            };
            await Modal.ShowAsync<VerificationModal>(title: "Bestätigen der E-Mail-Adresse", parameters: parameters);
        }

        public async Task ForgotPassword()
        {
            var closeModalFunc = Modal.HideAsync;
            var parameters = new Dictionary<string, object>
            {
                { nameof(ResetPassword.CloseModalFunc), closeModalFunc },
                { nameof(ResetPassword.Email), LoginData.Email }
            };
            await Modal.ShowAsync<ResetPassword>(title: "Password zurücksetzen", parameters: parameters);
        }

        public async Task SubmitLogin()
        {
            if (await LoginUser())
            {
                NavigationManager.TryNavigateToReturnUrl();
            }
        }

        public async Task SubmitRegister()
        {
            if (await RegisterUser())
            {
                NavigationManager.TryNavigateToReturnUrl();
            }
        }

        public void SignOut()
        {
            _authManager.RemoveLocalUser();
            NavigationManager.NavigateTo("./");
        }

        private async Task<bool> LoginUser()
        {
            try
            {
                var credetials = await _authManager.Authenticate(LoginData.Email, LoginData.Password, null, false);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unknown registration error: {ex.Message}, {LoginData.Email}, {LoginData.Password}");
                return false;

            }
        }

        private async Task<bool> RegisterUser()
        {
            try
            {
                var credentials = await _authManager.Authenticate(RegisterData.Email, RegisterData.Password, $"{RegisterData.FirstName} {RegisterData.LastName}", true);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unknown registration error: {ex.Message}, {RegisterData.Email}, {RegisterData.Password}, {RegisterData.FirstName}, {RegisterData.LastName}");
                return false;
            }

        }

        public class LoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class RegisterModel
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
