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

        public bool IsPreselectedOrAvailable { get => Helper.PreselectedMembers.Union(Helper.AvailableMembers).Contains(currentUserId); set { } }

        public bool IsMemberPermitted => Member.GroupIds.Intersect(Helper?.RequiredGroups ?? new()).Any() && Member.RoleIds.Contains(Helper?.RoleId);

        public bool IsPlayerLocked => Helper.LockedMembers.Contains(currentUserId);

        [Inject]
        private IAuthManager _authManager { get; set; }

        [Inject]
        private IHelperService _helperService { get; set; }

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


        public async Task ChangeHelpingEvent()
        {
            if(Helper.LockingTime < DateTime.Now && !IsPreselectedOrAvailable)
            {
                var closeModalFunc = Modal.HideAsync;
                var confirmModalAction = async () =>
                {
                    await SetIsAvailable(true);
                };
                var parameters = new Dictionary<string, object>
                {
                    { nameof(LockingHelperPrompt.LockingHelperPrompt.CloseModalFunc), closeModalFunc },
                    { nameof(LockingHelperPrompt.LockingHelperPrompt.ConfirmModalAction), confirmModalAction },
                };
                await Modal.ShowAsync<LockingHelperPrompt.LockingHelperPrompt>(title: "Rolle bestätigen", parameters: parameters);
            }
            else
            {
                await SetIsAvailable(!IsPreselectedOrAvailable);
            }            
        }
    }
}
