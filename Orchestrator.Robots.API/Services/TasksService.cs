using Orchestrator.Core.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Orchestrator.Robots.API.Services
{
    public class TasksService : ITasksService
    {
        readonly AppDbContext _context;

        public TasksService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateTaskAsync(string inputData, string name)
        {
            var task = new Core.Models.Task
            {
                Name = name,
                InputData = inputData,
                OutputData = string.Empty,
                TaskStatusId = 1,
                RobotId = System.Guid.Empty,
            };

            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Core.Models.Task>> GetAllTasksAsync()
        {
            return await _context.Tasks.ToListAsync();
        }

        public async Task<Core.Models.Task?> GetTaskByIdAsync(System.Guid id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<bool> SubmitTaskResultsAsync(System.Guid id, string outputData, int statusId)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;
            task.OutputData = outputData ?? string.Empty;
            task.TaskStatusId = statusId;

            if (task.RobotId != Guid.Empty && IsTerminalStatus(statusId))
            {
                var robot = await _context.Robots.FindAsync(task.RobotId);
                if (robot != null)
                {
                    robot.RobotStatusId = 1;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTaskStatusAsync(System.Guid id, int statusId)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;
            task.TaskStatusId = statusId;

            if (task.RobotId != Guid.Empty && IsTerminalStatus(statusId))
            {
                var robot = await _context.Robots.FindAsync(task.RobotId);
                if (robot != null)
                {
                    robot.RobotStatusId = 1;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private static bool IsTerminalStatus(int statusId)
        {
            return statusId == 3 || statusId == 4 || statusId == 5;
        }
    }
}
