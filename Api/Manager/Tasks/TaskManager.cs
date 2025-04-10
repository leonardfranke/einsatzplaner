﻿using GTask = Google.Cloud.Tasks.V2;

namespace Api.Manager
{
    public class TaskManager : ITaskManager
    {
        private GTask.CloudTasksClient _client;
        private string _optimizerEndPoint;

        public TaskManager(GTask.CloudTasksClient client, string optimizerEndPoint)
        {
            _client = client;
            _optimizerEndPoint = optimizerEndPoint;
        }

        public Task TriggerRecalculation(string departmentId, DateTime? dateTime = null)
        {
            if (dateTime == null) 
            {
                var berlinTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                dateTime = TimeZoneInfo.ConvertTime(DateTime.Today.AddDays(1), berlinTimeZone);
            }

            var httpRequest = new GTask.HttpRequest() 
            { 
                Url = $"{_optimizerEndPoint}?departmentId={departmentId}", 
                HttpMethod = GTask.HttpMethod.Get 
            };

            var queueName = new GTask.QueueName("einsatzplaner", "europe-west1", "Optimization");
            var taskId = $"{departmentId}-{dateTime.Value.ToString("yyyy-MM-dd")}";
            var taskName = new GTask.TaskName(queueName.ProjectId, queueName.LocationId, queueName.QueueId, taskId);
            var googleTask = new GTask.Task()
            {
                HttpRequest = httpRequest,
                TaskName = taskName
            };
            var secondsSinceEpoch = new DateTimeOffset(dateTime.Value.ToUniversalTime()).ToUnixTimeSeconds();
            googleTask.ScheduleTime = new Google.Protobuf.WellKnownTypes.Timestamp { Seconds = secondsSinceEpoch };
            return _client.CreateTaskAsync(queueName, googleTask);
        }
    }
}
