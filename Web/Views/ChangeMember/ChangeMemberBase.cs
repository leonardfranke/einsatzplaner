using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Web.Checks;
using Web.Manager;
using Web.Models;

namespace Web.Views
{
    public class ChangeMemberBase : ComponentBase
    {
        [Parameter]
        public string DepartmentId { get; set; }

        [Parameter]
        public Member Member { get; set; }

        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        [Parameter]
        public Func<string, Task> DeleteMemberFunc { get; set; }

        [Parameter]
        public Func<string, bool?, Task> SaveMemberFunc { get; set; }

        [Inject]
        private IDepartmentUrlCheck _departmentUrlCheck { get; set; }

        [Inject]
        public IAuthManager _authManager { private get; set; }

        private bool _oldIsAdmin;

        public ElementReference GroupSelect;

        [SupplyParameterFromForm]
        public FormModel MemberData { get; set; }
        public EditContext EditContext { get; set; }

        public bool IsMemberSaving { get; set; }
        public bool IsMemberDeleting { get; set; }
        public bool IsMemberLoading { get; set; }

        protected override void OnInitialized()
        {
            MemberData = new();
            EditContext = new(MemberData);
            EditContext.OnValidationRequested += ValidateForm;
        }

        protected override async Task OnInitializedAsync()
        {
            IsMemberLoading = true;
            IsMemberLoading = false;
        }

        private void ValidateForm(object? sender, ValidationRequestedEventArgs e)
        {

        }

        protected override void OnParametersSet()
        {
            MemberData = new FormModel();
            MemberData.IsAdmin = Member.IsAdmin;
            _oldIsAdmin = MemberData.IsAdmin;
        }

        public async Task SaveMember()
        {
            IsMemberSaving = true;
            bool? newIsAdmin = _oldIsAdmin != MemberData.IsAdmin ? MemberData.IsAdmin : null;
            if (newIsAdmin != null)
                await SaveMemberFunc(Member.Id, newIsAdmin);
            await CloseModal();
            IsMemberSaving = false;
        }

        public async Task DeleteMember()
        {
            IsMemberDeleting = true;
            await DeleteMemberFunc(Member.Id);
            await CloseModal();
            IsMemberDeleting = false;
        }

        public Task CloseModal() => CloseModalFunc();

        public class FormModel
        {
            public bool IsAdmin { get; set; }
        }
    }
}
