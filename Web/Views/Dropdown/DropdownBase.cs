using Microsoft.AspNetCore.Components;

namespace Web.Views
{
    public class DropdownBase<ItemType> : ComponentBase
    {
        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public string EmptyListText { get; set; }

        [Parameter]
        public IEnumerable<ItemType> Items { get; set; }

        [Parameter]
        public Func<ItemType, string> ItemDisplayFunc { get; set; }

        [Parameter]
        public Action<ItemType> ItemClickFunc { get; set; }
    }
}
