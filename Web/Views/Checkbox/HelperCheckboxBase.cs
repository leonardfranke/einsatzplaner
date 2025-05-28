using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Manager;
using Web.Services;

namespace Web.Views
{
    public class HelperCheckboxBase : ComponentBase
    {
        [CascadingParameter]
        public Modal Modal { get; set; }

        [Parameter]
        public Models.Event Event{ private get; set; }

        [Parameter]
        public Models.Helper Helper { get; set; }

        [Parameter]
        public Models.Member Member { private get; set; }

        public bool IsPreselectedOrAvailable { get => IsPreselected || IsAvailable;  set { } }

        public bool IsMemberPermitted => (Helper?.RequiredGroups.Count() == 0 || Member.GroupIds.Intersect(Helper?.RequiredGroups ?? new()).Any()) && Member.RoleIds.Contains(Helper?.RoleId);

        public bool IsPlayerLocked => Helper.LockedMembers.Contains(currentUserId);

        public string BackgroundColor {
            get 
            {
                if (IsPlayerLocked)
                    return "red";
                else if (IsPreselected)
                    return "orange";
                else if (IsAvailable)
                    return "green";
                else
                    return "";
            }}

        [Inject]
        private IAuthManager _authManager { get; set; }

        [Inject]
        private IHelperService _helperService { get; set; }

        private bool IsPreselected => Helper.PreselectedMembers.Contains(currentUserId);
        private bool IsAvailable => Helper.AvailableMembers.Contains(currentUserId);

        private string currentUserId;

        protected override async Task OnInitializedAsync()
        {
            currentUserId = (await _authManager.GetLocalUser()).Id;
        }

        protected Task<bool> SetIsAvailable(bool setHelping)
        {
            if(setHelping)
            {
                Helper.AvailableMembers.Add(currentUserId);
            }
            else
            {
                Helper.PreselectedMembers.Remove(currentUserId);
                Helper.AvailableMembers.Remove(currentUserId);
            }
            StateHasChanged();
            return _helperService.SetIsAvailable(Event.DepartmentId, Event.Id, Helper.Id, currentUserId, setHelping);
        }


        public Task ChangeHelpingEvent()
        {
            return SetIsAvailable(!IsPreselectedOrAvailable);         
        }
    }
}
