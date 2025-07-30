using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Manager;
using Web.Services;

namespace Web.Views
{
    public class HelperCheckboxBase : ComponentBase
    {
        private IEnumerable<Models.Group> _requiredGroups;

        [CascadingParameter]
        public Modal Modal { get; set; }

        [Parameter]
        public Action ValueChangedCallback { get; set; }

        [Parameter]
        public Models.Event Event{ private get; set; }

        [Parameter]
        public Models.Helper Helper { get; set; }

        [Parameter]
        public Models.Member Member { private get; set; }

        [Parameter]
        public Models.Role Role { private get; set; }

        [Parameter]
        public IEnumerable<Models.Group> Groups { private get; set; }

        public bool IsPreselectedOrAvailable { get => IsPreselected || IsAvailable;  set { } }

        public bool IsMemberPermitted => Role != null && _requiredGroups != null 
            && (_requiredGroups.Count() == 0 || _requiredGroups.SelectMany(group => group.MemberIds).Contains(Member.Id)) 
            && (Role.MemberIds.Contains(Member.Id) || Role.IsFree);

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

        protected override void OnParametersSet()
        {
            _requiredGroups = Groups.Where(group => Helper.RequiredGroups.Contains(group.Id));
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
            ValueChangedCallback?.Invoke();
            return SetIsAvailable(!IsPreselectedOrAvailable);         
        }
    }
}
