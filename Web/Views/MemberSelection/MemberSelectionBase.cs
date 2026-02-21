using Microsoft.AspNetCore.Components;
using Web.Models;

namespace Web.Views
{
    public class MemberSelectionBase : ComponentBase
    {
        [Parameter]
        public IEnumerable<Member> Members { get; set; } = new List<Member>();

        [Parameter]
        public SortedDictionary<string, Func<Member, bool>> GroupingFunctions { get; set; } = new();

        [Parameter]
        public List<string> SelectedMembers { get; set; } = new();

        public IEnumerable<IGrouping<string, Member>> GroupedMembers { get; private set; }

        protected override void OnParametersSet()
        {
            GroupedMembers = Members.GroupBy(member =>
            {
                foreach(var (key, func) in GroupingFunctions)
                {
                    if(func(member))
                    {
                        return key;
                    }
                }
                return string.Empty;
            });
        }

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
