using Microsoft.AspNetCore.Components;
using System.Text.Json.Nodes;

namespace Web.Navigation
{
    public static class NavigationManagerExtensions
    {
        public static bool TryNavigateToReturnUrl(this NavigationManager navigationManager)
        {
            if (navigationManager.HistoryEntryState is string historyState)
            {
                var entryStateJson = JsonNode.Parse(historyState);
                var returnUrl = entryStateJson?["returnUrl"]?.GetValue<string>();
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    navigationManager.NavigateTo(returnUrl);
                    return true;
                }
            }
            return false;
        }
    }
}
