using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Web.Views
{
    public class YesNoModalBase : ComponentBase
    {
        [CascadingParameter]
        public IMudDialogInstance MudDialog { get; set; }

        [Parameter]
        public string Text { get; set; }

        [Parameter]
        public string TrueButtonText { get; set; }

        [Parameter]
        public string FalseButtonText { get; set; }

        [Parameter]
        public Func<bool, Task> ResultFunc { get; set; }

        [Parameter]
        public Func<Task> CloseModalFunc { get; set; }

        public async Task SendResult(bool result)
        {
            MudDialog.Close(result);
        }
    }
}
