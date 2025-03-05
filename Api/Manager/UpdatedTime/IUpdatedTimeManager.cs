namespace Api.Manager
{
    public interface IUpdatedTimeManager
    {
        public Task<DateTime> GetHelperCategory(string depeartmentId);
        public Task SetHelperCategory(string depeartmentId);
        public Task<DateTime> GetHelperCategoryGroup(string depeartmentId);
        public Task SetHelperCategoryGroup(string depeartmentId);

    }
}
