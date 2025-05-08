using Google.Api.Gax.Grpc;
using GTask = Google.Cloud.Tasks.V2;

namespace Api.Manager.Tasks
{
    public class CloudTaskMock : GTask.CloudTasksClient
    {
        public override Task<GTask.Task> CreateTaskAsync(GTask.QueueName parent, GTask.Task task, CallSettings callSettings = null)
        {
            return Task.FromResult<GTask.Task>(null);
        }
    }
}
