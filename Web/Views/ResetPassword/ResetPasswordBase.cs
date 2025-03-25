using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Web.Manager;

namespace Web.Views
{
    public class ResetPasswordBase : ComponentBase
    {

        [Inject]
        public IAuthManager _authManager { private get; set; }

        [Parameter]
        public string Email { private get; set; }

        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        [SupplyParameterFromForm]
        public FormModel ResetData { get; set; }

        public EditContext EditContext { get; set; }

        private ValidationMessageStore _messageStore;

        protected override void OnInitialized()
        {
            ResetData = new();
            EditContext = new(ResetData);
            _messageStore = new(EditContext);
        }

        protected override void OnParametersSet()
        {
            ResetData.Email = Email;
        }

        public async Task ResetPassword()
        {
            await _authManager.ResetPassword(ResetData.Email);
            await CloseModalFunc();
        }

        public class FormModel
        {
            public string Email { get; set; }
        }
    }
}
