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

        public static string GetRegisteredMembers(this Models.Helper helper, IEnumerable<Member> members)
        {
            var str = "";
            if (helper.LockedMembers.Any())
                str += $"Fest: {string.Join(", ", helper.LockedMembers.Select(memberId => members.First(member => member.Id == memberId)).Select(member => member.Name))}{Environment.NewLine}";
            if (helper.PreselectedMembers.Any())
                str += $"Ausgewählt: {string.Join(", ", helper.PreselectedMembers.Select(memberId => members.First(member => member.Id == memberId)).Select(member => member.Name))}{Environment.NewLine}";
            if (helper.AvailableMembers.Any())
                str += $"Verfügbar: {string.Join(", ", helper.AvailableMembers.Select(memberId => members.First(member => member.Id == memberId)).Select(member => member.Name))}{Environment.NewLine}";
            return str;
        }

    }
}
