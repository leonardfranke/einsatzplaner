namespace Api.Manager
{
    public interface ITaskManager
    {
        public Task TriggerRecalculation(string departmentId, DateTimeOffset? dateTimeOffset = null);
    }
}
