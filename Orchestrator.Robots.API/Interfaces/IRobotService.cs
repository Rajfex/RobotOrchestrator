using System;
using System.Threading.Tasks;

namespace Orchestrator.Robots.API.Interfaces
{
    public interface IRobotService
    {
        Task<Orchestrator.Core.Models.Robot?> GetByApiKeyAsync(string apiKey);
        Task<Orchestrator.Core.Models.Task?> GetNextTaskAsync(Orchestrator.Core.Models.Robot robot);
        Task<bool> SubmitTaskResultAsync(Guid taskId, string outputData, int taskStatusId, Orchestrator.Core.Models.Robot robot);
        Task<IEnumerable<Orchestrator.Core.Models.Robot>> GetAllAsync();
        Task<Orchestrator.Core.Models.Robot> CreateRobotAsync(string name, string? apiKey = null);
    }
}
