using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Web.Models;

namespace Web.Views
{
    public class ChangeEventCategoryBase : ComponentBase
    {
        [Parameter]
        public EventCategory? EventCategory { get; set; }

        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        [Parameter]
        public Func<string, Task> DeleteEventCategoryFunc { get; set; }

        [Parameter]
        public Func<string, string, Task> SaveEventCategoryFunc { get; set; }

        [SupplyParameterFromForm]
        public FormModel EventCategoryData { get; set; }
        public EditContext EditContext { get; set; }
        public bool IsEventCategorySaving { get; set; }
        public bool IsEventCategoryDeleting { get; set; }

        public bool IsUpdate { get; set; }

        private ValidationMessageStore _messageStore;
        private string _oldName;

        protected override void OnInitialized()
        {
            EventCategoryData = new();
            EditContext = new(EventCategoryData);
            EditContext.OnValidationRequested += ValidateForm;
            _messageStore = new(EditContext);
        }

        private void ValidateForm(object? sender, ValidationRequestedEventArgs e)
        {
            _messageStore.Clear();

            if (string.IsNullOrWhiteSpace(EventCategoryData.Name))
                _messageStore.Add(() => EventCategoryData.Name, "Die Eventkategorie benötigt einen Namen.");
        }

        protected override void OnParametersSet()
        {
            IsUpdate = EventCategory != null;
            if (!IsUpdate)
                return;

            EventCategoryData.Name = EventCategory?.Name;
            _oldName = EventCategoryData.Name;
        }

        public async Task SaveEventCategory()
        {
            IsEventCategorySaving = true;
            if(EventCategoryData.Name != _oldName)
                await SaveEventCategoryFunc(EventCategory?.Id, EventCategoryData.Name);
            await CloseModal();
            IsEventCategorySaving = false;
        }

        public async Task DeleteEventCategory()
        {
            IsEventCategoryDeleting = true;
            await DeleteEventCategoryFunc(EventCategory.Id);
            await CloseModal();
            IsEventCategoryDeleting = false;
        }

        public Task CloseModal() => CloseModalFunc();

        public class FormModel
        {
            public string Name { get; set; }
        }

    }
}
