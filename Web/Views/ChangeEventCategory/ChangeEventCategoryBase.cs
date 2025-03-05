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

        protected override async Task OnParametersSetAsync()
        {
            IsUpdate = EventCategory != null;
            if (!IsUpdate)
                return;

            EventCategoryData.Name = EventCategory?.Name;
            _oldName = EventCategoryData.Name;
        }

        public async Task SaveEventCategory()
        {
            if(EventCategoryData.Name != _oldName)
                await SaveEventCategoryFunc(EventCategory?.Id, EventCategoryData.Name);
            await CloseModal();
        }

        public async Task DeleteEventCategory()
        {
            await DeleteEventCategoryFunc(EventCategory.Id);
            await CloseModal();
        }

        public Task CloseModal() => CloseModalFunc();

        public class FormModel
        {
            public string Name { get; set; }
        }

    }
}
