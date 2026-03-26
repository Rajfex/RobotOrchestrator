using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Orchestrator.Core.Data;
using System.Text.Json;

namespace Orchestrator.Robots.API.Services
{
    public class RobotService : IRobotService
    {
        readonly AppDbContext _context;

        public RobotService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Orchestrator.Core.Models.Robot>> GetAllAsync()
        {
            return await _context.Robots.ToListAsync();
        }

        public async Task<Orchestrator.Core.Models.Robot?> GetByApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return null;
            }

            return await _context.Robots.FirstOrDefaultAsync(r => r.API_KEY == apiKey);
        }

        public async Task<Orchestrator.Core.Models.Task?> GetNextTaskAsync(Orchestrator.Core.Models.Robot robot)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.TaskStatusId == 1 && t.RobotId == robot.Id);

            if (task == null)
            {
                var unassignedTasks = await _context.Tasks
                    .Where(t => t.TaskStatusId == 1 && t.RobotId == Guid.Empty)
                    .ToListAsync();

                task = unassignedTasks
                    .FirstOrDefault(t => IsTaskSupportedByRobot(t.InputData, robot.Name));
            }

            if (task == null)
            {
                return null;
            }

            task.RobotId = robot.Id;
            task.TaskStatusId = 2;

            var dbRobot = await _context.Robots.FindAsync(robot.Id);
            if (dbRobot != null)
            {
                dbRobot.RobotStatusId = 2;
            }

            await _context.SaveChangesAsync();
            return task;
        }

        private static bool IsTaskSupportedByRobot(string inputData, string robotName)
        {
            if (string.IsNullOrWhiteSpace(robotName))
            {
                return false;
            }

            try
            {
                using var doc = JsonDocument.Parse(inputData);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                if (doc.RootElement.TryGetProperty("FootballLeagueInfo", out _))
                {
                    return robotName.Contains("flashscore", StringComparison.OrdinalIgnoreCase);
                }
                if (doc.RootElement.TryGetProperty("StocksInfo", out _))
                {
                    return robotName.Contains("stocks", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
            }

            return false;
        }

        public async Task<bool> SubmitTaskResultAsync(Guid taskId, string outputData, int taskStatusId, Orchestrator.Core.Models.Robot robot)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return false;
            }
            task.RobotId = robot.Id;
            task.OutputData = outputData ?? string.Empty;
            task.TaskStatusId = taskStatusId;

            if (taskStatusId == 3 || taskStatusId == 4 || taskStatusId == 5)
            {
                var dbRobot = await _context.Robots.FindAsync(robot.Id);
                if (dbRobot != null)
                {
                    dbRobot.RobotStatusId = 1;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Orchestrator.Core.Models.Robot> CreateRobotAsync(string name, string? apiKey = null)
        {
            var robot = new Orchestrator.Core.Models.Robot
            {
                Id = Guid.NewGuid(),
                Name = name,
                API_KEY = string.IsNullOrEmpty(apiKey) ? GenerateApiKey() : apiKey,
                RobotStatusId = 1
            };
            await _context.Robots.AddAsync(robot);
            await _context.SaveChangesAsync();
            return robot;
        }

        private string GenerateApiKey()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", "").Replace("+", "").Replace("/", "");
        }
    }
}
