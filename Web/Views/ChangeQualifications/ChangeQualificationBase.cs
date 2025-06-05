using Microsoft.AspNetCore.Components;
using Web.Models;

namespace Web.Views
{
    public class ChangeQualificationBase : ComponentBase
    {
        [Parameter]
        public Role Role { private get; set; }

        [Parameter]
        public Qualification Qualification { get; set; }

        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        [Parameter]
        public Func<string, Task> DeleteQualificationFunc { get; set; }

        [Parameter]
        public Func<string, string, string, Task> UpdateQualificationFunc { get; set; }

        public FormModel QualificationData { get; set; }

        public bool IsQualificationSaving { get; set; }
        public bool IsQualificationDeleting { get; set; }

        public bool IsUpdate { get; set; }

        private string _oldName;

        protected override void OnInitialized()
        {
            QualificationData = new();
        }

        protected override void OnParametersSet()
        {
            IsUpdate = Qualification != null;
            if (!IsUpdate)
            {
                _oldName = string.Empty;
                QualificationData.Name = _oldName;
            }
            else
            {
                QualificationData.Name = Qualification.Name;
                _oldName = QualificationData.Name;
            }
        }

        public async Task SaveGroup()
        {
            IsQualificationSaving = true;
            if (QualificationData.Name != _oldName)
                await UpdateQualificationFunc(Role?.Id, Qualification?.Id,QualificationData.Name);

            await CloseModal();
            IsQualificationSaving = false;
        }

        public async Task DeleteGroup()
        {
            IsQualificationDeleting = true;
            await DeleteQualificationFunc(Qualification.Id);
            await CloseModal();
            IsQualificationDeleting = false;
        }

        public Task CloseModal() => CloseModalFunc();

        public class FormModel
        {
            public string Name { get; set; }
        }

    }
}
