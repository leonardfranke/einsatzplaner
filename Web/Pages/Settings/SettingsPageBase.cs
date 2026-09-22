using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Web.Checks;
using Web.Manager;
using Web.Services;

namespace Web.Pages.Settings
{
    public class SettingsPageBase : ComponentBase
    {
        [Parameter]
        public string DepartmentUrl { get; set; }

        [Inject]
        private IDepartmentUrlCheck _departmentUrlCheck { get; set; }

        [Inject]
        private ILoginCheck _loginCheck { get; set; }

        [Inject]
        private IAuthManager _authManager { get; set; }

        [Inject]
        private IJSRuntime _jsRuntime { get; set; }

        [Inject]
        protected ToastService _toastService { get; set; }

        [Inject]
        private ICalendarService _calendarService { get; set; }

        public string? CalendarUrl { get; set; }

        public bool IsCalendarLoading { get; set; }

        private string _departmentId;
        private string _memberId;
        protected MudTextField<string> _urlTextField;

        protected override async Task OnInitializedAsync()
        {
            if (await _departmentUrlCheck.LogIntoDepartment(DepartmentUrl) is not Models.Department department)
                return;

            _departmentId = department.Id;
            if (!await _loginCheck.CheckLogin(DepartmentUrl, department, true))
                return;

            var user = await _authManager.GetLocalUser();
            if (string.IsNullOrEmpty(user?.Id))
                return;

            _memberId = user.Id;
            await LoadCalendarToken();
        }

        public async Task GenerateCalendarToken()
        {
            IsCalendarLoading = true;
            var token = await _calendarService.GenerateCalendarToken(_departmentId, _memberId);
            CalendarUrl = token?.Url;
            IsCalendarLoading = false;
        }

        public async Task InvalidateCalendarToken()
        {
            IsCalendarLoading = true;
            await _calendarService.InvalidateCalendarToken(_departmentId, _memberId);
            CalendarUrl = null;
            IsCalendarLoading = false;
        }

        private async Task LoadCalendarToken()
        {
            IsCalendarLoading = true;
            var token = await _calendarService.GetCalendarToken(_departmentId, _memberId);
            CalendarUrl = token?.Url;
            IsCalendarLoading = false;
        }

        protected async Task CopyToClipboard()
        {
            if (!string.IsNullOrEmpty(CalendarUrl))
            {
                await _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", CalendarUrl);
                _toastService.Notify(new ToastMessage(ToastType.Info, "URL kopiert"));
            }
        }
    }
}
