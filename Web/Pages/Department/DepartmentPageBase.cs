using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Web.Checks;
using Web.Manager;
using Web.Models;
using Web.Navigation;
using Web.Services;

namespace Web.Pages
{
    public class DepartmentPageBase : ComponentBase
    {
        [Parameter]
        public string DepartmentUrl { get; set; }

        [Inject]
        private ILoginCheck _loginCheck { get; set; }

        [Inject]
        private IDepartmentUrlCheck _departmentUrlCheck { get; set; }
        [Inject]
        private IAuthManager _authManager { get; set; }
        [Inject]
        private IDepartmentService _departmentService { get; set; }
        [Inject]
        private NavigationManager _navigationManager { get; set; }
        [Inject]
        private IJSRuntime _javaScript { get; set; }

        private User currentUser;
        public Models.Department Department { get; set; }
        public bool AlreadyRequested { get; private set; }

        protected override async Task OnInitializedAsync()
        {
            if (await _departmentUrlCheck.CheckDepartmentUrl(DepartmentUrl) is not Models.Department department)
                return;
            Department = department;
            if (!await _loginCheck.CheckLogin())
                return;

            currentUser = await _authManager.GetLocalUser();
            if(currentUser == null || string.IsNullOrWhiteSpace(currentUser.Id))
                throw new NullReferenceException("Currently authenticated user is null");

            var isInDepartment = await _departmentService.IsMemberInDepartment(currentUser.Id, Department.Id);
            if (isInDepartment)
            {
                var navigated = _navigationManager.TryNavigateToReturnUrl();
                if (!navigated)
                    _navigationManager.NavigateTo($"./{DepartmentUrl}");
            }
            else
            {
                AlreadyRequested = await _departmentService.MembershipRequested(Department.Id, currentUser.Id);
                await _javaScript.InvokeVoidAsync("openModal");
            }
        }

        public async Task CloseModal()
        {
            await _javaScript.InvokeVoidAsync("closeModal");
        }

        public async Task SubmitRequest()
        {
            var currentUser = await _authManager.GetLocalUser();
            if (currentUser == null)
                return;
            await _departmentService.RequestMembership(Department.Id, currentUser.Id);
            await CloseModal();
        }

        public class FormModel
        {
            public string DepartmentId { get; set; }
        }
    }
}
