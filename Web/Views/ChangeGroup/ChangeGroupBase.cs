using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Web.Checks;
using Web.Manager;
using Web.Models;
using Web.Services;

namespace Web.Views
{
    public class ChangeGroupBase : ComponentBase
    {
        [Parameter]
        public string DepartmentId { get; set; }

        [Parameter]
        public Group Group { get; set; }

        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        [Parameter]
        public Func<string, Task> DeleteGroupFunc { get; set; }

        [Inject]
        public IMemberService _memberService { private get; set; }

        [Parameter]
        public Func<string, string, Task> UpdateGroupFunc { get; set; }

        [SupplyParameterFromForm]
        public FormModel GroupData { get; set; }
        public EditContext EditContext { get; set; }

        public bool IsGroupSaving { get; set; }
        public bool IsGroupDeleting { get; set; }
        public bool IsGroupLoading { get; set; }

        public bool IsUpdate { get; set; }

        private ValidationMessageStore _messageStore;
        private string _oldName;

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

        protected override void OnParametersSet()
        {
            IsGroupLoading = true;
            IsUpdate = Group != null;
            if (!IsUpdate)
            {
                _oldName = string.Empty;
                GroupData.Name = _oldName;
            }
            else
            {
                GroupData.Name = Group.Name;
                _oldName = GroupData.Name;
            }
            IsGroupLoading = false;
        }

        public async Task SafeGroup()
        {
            IsGroupSaving = true;
            if (GroupData.Name != _oldName)
                await UpdateGroupFunc(Group?.Id, GroupData.Name);

            await CloseModal();
            IsGroupSaving = false;
        }

        public async Task DeleteGroup()
        {
            IsGroupDeleting = true;
            await DeleteGroupFunc(Group.Id);
            await CloseModal();
            IsGroupDeleting = false;
        }

        public Task CloseModal() => CloseModalFunc();

        public class FormModel
        {
            public string Name { get; set; }
        }

    }
}
