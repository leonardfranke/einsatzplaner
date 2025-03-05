using Microsoft.AspNetCore.Components;
using Web.Models;

namespace Web.Views
{
    public class MemberSelectionBase : ComponentBase
    {
        [Parameter]
        public List<Member> Members { protected get; set; } = new();

        [Parameter]
        public List<string> SelectedMembers { get; set; } = new();

        protected void SetMember(string memberId, bool isSet)
        {
            if (isSet)
                SelectedMembers.Add(memberId);
            else
                SelectedMembers.Remove(memberId);
        }

        protected bool GetIsSelected(string memberId)
        {
            return SelectedMembers.Contains(memberId);
        }
    }
}
