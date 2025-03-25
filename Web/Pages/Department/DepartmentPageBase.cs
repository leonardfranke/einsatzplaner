using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
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

        [Inject]
        private ILoginCheck _loginCheck { get; set; }
        [Inject]
        private IAuthManager _authManager { get; set; }
        [Inject]
        private IDepartmentService _departmentService { get; set; }
        [Inject]
        private NavigationManager _navigationManager { get; set; }
        [Inject]
        private IJSRuntime _javaScript { get; set; }

        private ValidationMessageStore _messageStore;
        private User currentUser;

        [SupplyParameterFromForm]
        public FormModel DepartmentData { get; set; }
        public EditContext EditContext { get; set; }
        public string RequestModalText { get; private set; }
        public Models.Department DepartmentRequest { get; private set; }
        public bool AlreadyRequested { get; private set; }


        public List<Models.Department> Departments { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {            
            DepartmentData = new();
            EditContext = new(DepartmentData);
            EditContext.OnValidationRequested += ValidateForm;
            _messageStore = new(EditContext);

            if (!await _loginCheck.CheckLogin())
                return;

            currentUser = await _authManager.GetLocalUser();
            if(currentUser == null || string.IsNullOrWhiteSpace(currentUser.Id))
                throw new NullReferenceException("Currently authenticated user is null");
            Departments = await _departmentService.GetAll();
            await base.OnInitializedAsync();
        }

        private void ValidateForm(object? sender, ValidationRequestedEventArgs e)
        {
            _messageStore.Clear();

            if (string.IsNullOrEmpty(DepartmentData.DepartmentId))
                _messageStore.Add(() => DepartmentData.DepartmentId, "Wähle eine Abteilung aus.");
        }

        public async Task SubmitChangeDepartment()
        {
            var selectedDepartment = Departments.Find(department => DepartmentData.DepartmentId == department.Id);
            if (selectedDepartment == null)
                throw new NullReferenceException("Internal error: selectedDepartment is null");
            var isInDepartment = await _departmentService.IsMemberInDepartment(currentUser.Id, selectedDepartment.Id);
            if (isInDepartment)
            {
                await _authManager.SetLocalDepartmentId(selectedDepartment.Id);
                var navigated = _navigationManager.TryNavigateToReturnUrl();
                if (!navigated)
                    _navigationManager.NavigateTo("./");
            }
            else
            {
                DepartmentRequest = selectedDepartment;
                AlreadyRequested = await _departmentService.MembershipRequested(DepartmentRequest.Id, currentUser.Id);
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
            await _departmentService.RequestMembership(DepartmentRequest.Id, currentUser.Id);
            await CloseModal();
        }

        public class FormModel
        {
            public string DepartmentId { get; set; }
        }
    }
}
