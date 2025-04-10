namespace Api.Manager
{
    public interface ITaskManager
    {
        public Task TriggerRecalculation(string departmentId, DateTime? dateTime = null);
    }
}
