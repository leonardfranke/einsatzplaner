using GTask = Google.Cloud.Tasks.V2;

namespace Api.Manager
{
    public class TaskManager : ITaskManager
    {
        private GTask.CloudTasksClient _client;
        private string _optimizerEndPoint;

        public TaskManager(string optimizerEndPoint)
        {
            _client = GTask.CloudTasksClient.Create();
            _optimizerEndPoint = optimizerEndPoint;
        }

        public Task TriggerRecalculation(string departmentId, DateTimeOffset? dateTimeOffset = null)
        {
            if (dateTimeOffset == null) 
            {
                var berlinTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
                dateTimeOffset = new DateTimeOffset(DateTime.Today.AddDays(1), berlinTimeZone.BaseUtcOffset);
            }

            var httpRequest = new GTask.HttpRequest() 
            { 
                Url = $"{_optimizerEndPoint}?departmentId={departmentId}", 
                HttpMethod = GTask.HttpMethod.Get 
            };
            var googleTask = new GTask.Task()
            {
                HttpRequest = httpRequest,
                TaskName = new GTask.TaskName("einsatzplaner", 
                    "europe-west1", 
                    "Optimization", 
                    $"{departmentId}{dateTimeOffset.Value.ToString("yyyy-MM-dd")}")
            };
            googleTask.ScheduleTime = new Google.Protobuf.WellKnownTypes.Timestamp { Seconds = dateTimeOffset.Value.ToUnixTimeSeconds() };
            return _client.CreateTaskAsync("Optimization", googleTask);
        }
    }
}
