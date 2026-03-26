namespace Orchestrator.Robots.API.Services
{
    public interface ITasksService
    {
        public Task<bool> CreateTaskAsync(string inputData, string name);
        public Task<IEnumerable<Core.Models.Task>> GetAllTasksAsync();
        public Task<Core.Models.Task?> GetTaskByIdAsync(System.Guid id);
        public Task<bool> SubmitTaskResultsAsync(System.Guid id, string outputData, int statusId);
        public Task<bool> UpdateTaskStatusAsync(System.Guid id, int statusId);
    }
}
