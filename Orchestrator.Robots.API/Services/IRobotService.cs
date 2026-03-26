using System;
using System.Threading.Tasks;

namespace Orchestrator.Robots.API.Services
{
    public interface IRobotService
    {
        Task<Core.Models.Robot?> GetByApiKeyAsync(string apiKey);
        Task<Core.Models.Task?> GetNextTaskAsync(Core.Models.Robot robot);
        Task<bool> SubmitTaskResultAsync(Guid taskId, string outputData, int taskStatusId, Core.Models.Robot robot);
        Task<IEnumerable<Core.Models.Robot>> GetAllAsync();
        Task<Core.Models.Robot> CreateRobotAsync(string name, string? apiKey = null);
    }
}
