using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Web.Manager;
using Web.Models;
using Web.Services;

namespace Web.Views
{
    public class ChangeGroupBase : ComponentBase
    {
        [Parameter]
        public Group Group { get; set; }

        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        [Parameter]
        public Func<string, Task> DeleteGroupFunc { get; set; }

        [Inject]
        public IMemberService _memberService { private get; set; }

        [Inject]
        private IAuthManager _authManager { get; set; }

        [Parameter]
        public Func<string, string, IEnumerable<string>, IEnumerable<string>, Task> UpdateGroupFunc { get; set; }

        [SupplyParameterFromForm]
        public FormModel GroupData { get; set; }
        public EditContext EditContext { get; set; }

        protected List<string> SelectedMembers { get; private set; }

        protected List<Member> Members { get; private set; } = new();

        public bool IsUpdate { get; set; }

        private ValidationMessageStore _messageStore;
        private string _oldName;
        private List<string> _oldSelectedMembers;

        protected override void OnInitialized()
        {
            GroupData = new();
            EditContext = new(GroupData);
            EditContext.OnValidationRequested += ValidateForm;
            _messageStore = new(EditContext);
        }

        private void ValidateForm(object? sender, ValidationRequestedEventArgs e)
        {
            _messageStore.Clear();

            if (string.IsNullOrWhiteSpace(GroupData.Name))
                _messageStore.Add(() => GroupData.Name, "Das Group benötigt einen Namen.");
        }

        protected override async Task OnParametersSetAsync()
        {
            IsUpdate = Group != null;
            if (!IsUpdate)
            {
                _oldName = string.Empty;
                GroupData.Name = _oldName;
                _oldSelectedMembers = new();
                SelectedMembers = new();
                return;
            }

            var departmentId = await _authManager.GetLocalDepartmentId();
            Members = await _memberService.GetAll(departmentId);
            _oldSelectedMembers = Members.Where(member => member.GroupIds.Contains(Group.Id)).Select(member => member.Id).ToList();
            SelectedMembers = new(_oldSelectedMembers);
            GroupData.Name = Group.Name;
            _oldName = GroupData.Name;
        }

        public async Task SafeGroup()
        {
            var newGroupMembers = SelectedMembers.Except(_oldSelectedMembers);
            var formerGroupMembers = _oldSelectedMembers.Except(SelectedMembers);
            if (GroupData.Name != _oldName || newGroupMembers.Any() || formerGroupMembers.Any())
                await UpdateGroupFunc(Group?.Id, GroupData.Name, newGroupMembers, formerGroupMembers);

            await CloseModal();
        }

        public async Task DeleteGroup()
        {
            await DeleteGroupFunc(Group.Id);
            await CloseModal();
        }

        public Task CloseModal() => CloseModalFunc();

        public class FormModel
        {
            public string Name { get; set; }
        }

    }
}
