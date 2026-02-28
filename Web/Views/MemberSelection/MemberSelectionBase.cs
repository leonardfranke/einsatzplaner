using Microsoft.AspNetCore.Components;
using MudBlazor;
using Web.Models;

namespace Web.Views
{
    public class MemberSelectionBase : ComponentBase
    {
        [CascadingParameter]
        public IMudDialogInstance MudDialog { get; set; }

        [Parameter]
        public IEnumerable<Member> Members { get; set; }

        [Parameter]
        public List<(string, Func<Member, bool>)> GroupingFunctions { get; set; } = new();

        [Parameter]
        public List<string> SelectedMembers { get; set; }

        public List<string> ResultMembers { get; set; }

        public TableGroupDefinition<Member> GroupDefinition { get; set; }

        public IEnumerable<Member> OrderedMembers { get; set; }

        protected override void OnParametersSet()
        {
            ResultMembers = [.. SelectedMembers];
            OrderedMembers = Members.OrderBy(member =>
            {
                foreach (var ((key, func), i) in GroupingFunctions.Select((val, i) => (val, i)))
                {
                    if (func(member))
                    {
                        return i;
                    }
                }
                return GroupingFunctions.Count;
            });
            var selector = (Member member) =>
            {
                foreach (var (key, func) in GroupingFunctions)
                {
                    if (func(member))
                    {
                        return key;
                    }
                }
                return "Weitere";
            };

            GroupDefinition = new TableGroupDefinition<Member>
            {
                Selector = selector
            };
        }

        protected void SetMember(string memberId, bool isSet)
        {
            if (isSet)
                ResultMembers.Add(memberId);
            else
                ResultMembers.Remove(memberId);
        }

        protected bool GetIsSelected(string memberId)
        {
            return ResultMembers.Contains(memberId);
        }

        public void Submit()
        {
            MudDialog.Close(DialogResult.Ok(ResultMembers));
        }
    }
}
