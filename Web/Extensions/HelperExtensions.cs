namespace Web.Extensions
{
    public static class HelperExtensions
    {
        public static string GetRegistrationCount(this Models.Helper helper)
        {
            var registrations = helper.LockedMembers.Union(helper.PreselectedMembers).Union(helper.AvailableMembers).Count();
            return $"{registrations}/{helper.RequiredAmount}";
        }

    }
}
