using Microsoft.AspNetCore.Components;

namespace Web.Views
{
    public class LockingHelperPromptBase : ComponentBase
    {
        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        [Parameter]
        public Func<Task> ConfirmModalAction { private get; set; }

        protected async Task ConfirmModal()
        {
            await ConfirmModalAction();
            await CloseModalFunc();
        }
    }
}
