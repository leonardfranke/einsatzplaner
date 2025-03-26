using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Checks;
using Web.Manager;
using Web.Services;
using Web.Views.ChangeEventCategory;

namespace Web.Pages
{
    public class EventCategoryPageBase : ComponentBase
    {
        private string _departmentId;

        [Parameter]
        public string DepartmentUrl { get; set; }

        public List<Models.EventCategory> EventCategories { get; set; }

        [Inject]
        private IEventCategoryService _eventCategoryService { get; set; }

        [Inject]
        private ILoginCheck _loginCheck { get; set; }

        [Inject]
        private IAuthManager _authManager { get; set; }
        [Inject]
        private IDepartmentUrlCheck _departmentUrlCheck { get; set; }

        public int? HoveredIndex { get; set; }

        public List<Models.Member> Members { get; set; }

        [CascadingParameter]
        public Modal Modal { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (await _departmentUrlCheck.LogIntoDepartment(DepartmentUrl) is not Models.Department department)
                return;
            if (!await _loginCheck.CheckLogin(department))
                return;
            _departmentId = department.Id;
            await LoadEventCategories();
        }

        private async Task LoadEventCategories()
        {
            EventCategories = await _eventCategoryService.GetAll(_departmentId);
        }

        public async Task EditOrCreateEventCategory(Models.EventCategory? eventCategory)
        {
            var closeModalFunc = Modal.HideAsync;
            var deleteEventCategoryFunc = DeleteEventCategory;
            var saveEventCategoryFunc = SaveEventCategory;
            var parameters = new Dictionary<string, object>
            {
                { nameof(ChangeEventCategory.EventCategory), eventCategory },
                { nameof(ChangeEventCategory.CloseModalFunc), closeModalFunc },
                { nameof(ChangeEventCategory.DeleteEventCategoryFunc), deleteEventCategoryFunc },
                { nameof(ChangeEventCategory.SaveEventCategoryFunc), saveEventCategoryFunc }
            };
            var title = eventCategory == null ? "Eventkategorie erstellen" : "Eventkategorie bearbeiten";
            await Modal.ShowAsync<ChangeEventCategory>(title: title, parameters: parameters);
        }

        private async Task DeleteEventCategory(string eventCategoryId)
        {
            await _eventCategoryService.Delete(_departmentId, eventCategoryId);
            await LoadEventCategories();
        }

        private async Task SaveEventCategory(string eventCategoryId, string name)
        {
            await _eventCategoryService.UpdateOrCreate(_departmentId, eventCategoryId, name);
            await LoadEventCategories();
        }
    }
}
