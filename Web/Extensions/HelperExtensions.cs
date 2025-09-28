using Microsoft.AspNetCore.Components;
using Web.Models;

namespace Web.Extensions
{
    public static class HelperExtensions
    {
        public static string GetRegistrationCount(this Models.Helper helper)
        {
            var registrations = helper.LockedMembers.Union(helper.PreselectedMembers).Union(helper.AvailableMembers).Count();
            return $"{registrations}/{helper.RequiredAmount}";
        }

        public static MarkupString GetRegisteredMembers(this Models.Helper helper, IEnumerable<Member> members)
        {
            var unknownUserString = "Unbekannter Nutzer";
            var str = "";
            if (helper.LockedMembers.Any())
                str += $"Fest: {string.Join(", ", helper.LockedMembers.Select(memberId => members.FirstOrDefault(member => member.Id == memberId)).Select(member => member?.Name ?? unknownUserString))}<br/>";
            if (helper.PreselectedMembers.Any())
                str += $"Ausgewählt: {string.Join(", ", helper.PreselectedMembers.Select(memberId => members.FirstOrDefault(member => member.Id == memberId)).Select(member => member?.Name ?? unknownUserString))}<br/>";
            if (helper.AvailableMembers.Any())
                str += $"Verfügbar: {string.Join(", ", helper.AvailableMembers.Select(memberId => members.FirstOrDefault(member => member.Id == memberId)).Select(member => member?.Name ?? unknownUserString))}<br/>";
            return new MarkupString(str);
        }

    }
}
