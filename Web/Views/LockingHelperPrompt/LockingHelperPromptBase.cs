using Microsoft.AspNetCore.Components;

namespace Web.Views
{
    public class LockingHelperPromptBase : ComponentBase
    {
        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        [Parameter]
        public Action ConfirmModalAction { private get; set; }

        protected async Task ConfirmModal()
        {
            ConfirmModalAction();
            await CloseModalFunc();
        }
    }
}
