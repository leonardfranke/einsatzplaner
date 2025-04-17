using Microsoft.AspNetCore.Components;
using Web.Models;

namespace Web.Views
{
    public class MemberSelectionModalBase : ComponentBase
    {
        [Parameter]
        public Func<Task> ConfirmModalAction { private get; set; }

        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        [Parameter]
        public List<Member> Members { get; set; } = new();

        [Parameter]
        public List<string> SelectedMembers { get; set; } = new();

        protected async Task ConfirmModal()
        {
            await ConfirmModalAction();
            await CloseModalFunc();
        }
    }
}
