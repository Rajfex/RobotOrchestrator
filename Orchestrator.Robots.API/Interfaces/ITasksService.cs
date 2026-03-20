namespace Orchestrator.Robots.API.Interfaces
{
    public interface ITasksService
    {
        public Task<bool> CreateTaskAsync(string inputData);
        public Task<IEnumerable<Orchestrator.Core.Models.Task>> GetAllTasksAsync();
        public Task<Orchestrator.Core.Models.Task?> GetTaskByIdAsync(System.Guid id);
        public Task<bool> SubmitTaskResultsAsync(System.Guid id, string outputData, int statusId);
        public Task<bool> UpdateTaskStatusAsync(System.Guid id, int statusId);
    }
}
