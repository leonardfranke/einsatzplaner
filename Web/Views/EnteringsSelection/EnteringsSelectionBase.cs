using Microsoft.AspNetCore.Components;
using MudBlazor;
using Web.Models;

namespace Web.Views
{
    public class EnteringsSelectionBase : ComponentBase
    {
        [CascadingParameter]
        public IMudDialogInstance MudDialog { get; set; }

        [Parameter]
        public IEnumerable<Member> Members { get; set; }

        [Parameter]
        public IEnumerable<string> LockedMembers { get; set; }

        [Parameter]
        public IEnumerable<string> PreselectedMembers { get; set; }

        [Parameter]
        public IEnumerable<string> AvailableMembers { get; set; }

        [Parameter]
        public IEnumerable<string> RecommendedMembers { get; set; }

        private List<string> ShowAsLocked { get; set; }
        private List<string> ShowAsAvailable { get; set; }
        public Dictionary<string, List<Member>> GroupedMembers { get; set; }

        protected override void OnInitialized()
        {
            var lockedKey = "Bereits zugewiesen";
            var preselectedKey = "Vorausgewählt";
            var availableKey = "Verfügbar";
            var recommendedKey = "Empfehlungen";
            var othersKey = "Weitere";

            GroupedMembers = new Dictionary<string, List<Member>>() 
                { { lockedKey, [] }, { preselectedKey, [] }, { availableKey, [] }, { recommendedKey, [] }, { othersKey, [] } };
            foreach(var member in Members)
            {
                if (LockedMembers.Contains(member.Id))
                    GroupedMembers[lockedKey].Add(member);
                else if (PreselectedMembers.Contains(member.Id))
                    GroupedMembers[preselectedKey].Add(member);
                else if (AvailableMembers.Contains(member.Id))
                    GroupedMembers[availableKey].Add(member);
                else if (RecommendedMembers.Contains(member.Id))
                    GroupedMembers[recommendedKey].Add(member);
                else
                    GroupedMembers[othersKey].Add(member);
            }

            ShowAsLocked = [.. LockedMembers];
            ShowAsAvailable = [.. PreselectedMembers, .. AvailableMembers];
        }

        protected Member GetMember(string memberId)
        {
            return Members.FirstOrDefault(member => member.Id == memberId);
        }

        protected string GetEnteringType(string memberId)
        {
            if (ShowAsLocked.Contains(memberId))
                return "Locked";
            else if (ShowAsAvailable.Contains(memberId))
                return "Available";
            return null;
        }

        protected void SetEnteringType(string memberId, string enteringType)
        {
            if(enteringType == "Locked")
            {
                ShowAsLocked.Add(memberId);
                ShowAsAvailable.Remove(memberId);
            }
            else if(enteringType == "Available")
            {
                ShowAsLocked.Remove(memberId);
                ShowAsAvailable.Add(memberId);
            }
            else if(string.IsNullOrEmpty(enteringType))
            {
                ShowAsLocked.Remove(memberId);
                ShowAsAvailable.Remove(memberId);
            }
        }

        public void Submit()
        {
            var newLockedMembers = ShowAsLocked.Except(LockedMembers).ToList();
            var newAvailableMembers = ShowAsAvailable.Except(PreselectedMembers).Except(AvailableMembers).ToList();
            var removedMembers = LockedMembers.Union(PreselectedMembers).Union(AvailableMembers).Except(RecommendedMembers).Except(ShowAsLocked).Except(ShowAsAvailable).ToList();

            MudDialog.Close(DialogResult.Ok((newLockedMembers, newAvailableMembers, removedMembers)));
        }
    }
}
