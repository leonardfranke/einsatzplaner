using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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

        private bool _isHelping;

        public bool IsHelping { 
            get => _isHelping; 
            set
            {
                _isHelping = value;
                SetIsHelping(value);
            }
        }

        public bool IsMemberPermitted => Member.GroupIds.Intersect(Helper?.RequiredGroups ?? new()).Any() && Member.RoleIds.Contains(Helper?.RoleId);

        public bool IsPlayerLocked => (Helper?.SetMemberIds?.Contains(currentUserId) == true) && Helper?.LockingTime < DateTime.Now;

        [Inject]
        private IAuthManager _authManager { get; set; }

        [Inject]
        private IHelperService _helperService { get; set; }

        private string currentUserId;

        protected override async Task OnInitializedAsync()
        {
            currentUserId = (await _authManager.GetLocalUser()).Id;
        }

        protected override async Task OnParametersSetAsync()
        {            
            var queuedMembers = Helper.QueuedMemberIds ?? new();
            var setMembers = Helper.SetMemberIds ?? new();
            _isHelping = queuedMembers.Union(setMembers).Contains(currentUserId);
        } 

        protected Task<bool> SetIsHelping(bool value)
        {
            return _helperService.SetIsHelping(Event.DepartmentId, Event.Id, Helper.Id, currentUserId, value);
        }

        public async Task PromptLockingHelper()
        {
            var closeModalFunc = Modal.HideAsync;
            var confirmModalAction = () =>
            {
                Helper?.SetMemberIds.Add(currentUserId);
                IsHelping = true;
            };
            var parameters = new Dictionary<string, object>
            {
                { nameof(LockingHelperPrompt.LockingHelperPrompt.CloseModalFunc), closeModalFunc },
                { nameof(LockingHelperPrompt.LockingHelperPrompt.ConfirmModalAction), confirmModalAction },
            };
            await Modal.ShowAsync<LockingHelperPrompt.LockingHelperPrompt>(title: "Rolle bestätigen", parameters: parameters);
        }
    }
}
