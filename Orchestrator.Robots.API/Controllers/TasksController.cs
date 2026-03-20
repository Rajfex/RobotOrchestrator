using Microsoft.AspNetCore.Mvc;
using Orchestrator.Robots.API.Interfaces;
using System.Threading.Tasks;
using System.Text.Json;
using Orchestrator.Core;

namespace Orchestrator.Robots.API.Controllers
{
    public class CreateTaskRequest
    {
        public JsonElement InputData { get; set; }
    }

    public class ApiTasksBody
    {
        public System.Guid? RobotId { get; set; }
        public JsonElement InputData { get; set; }
    }

    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly ITasksService _tasksService;
        private readonly Logger _logger;

        public TasksController(ITasksService tasksService, Logger logger)
        {
            _tasksService = tasksService;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
        {
            if (request == null || request.InputData.ValueKind == JsonValueKind.Undefined || request.InputData.ValueKind == JsonValueKind.Null)
            {
                _logger.Log(Guid.Empty, "[POST] task creation failed", 2);
                return BadRequest("Input data is required.");
            }
            var json = request.InputData.GetRawText();
            var success = await _tasksService.CreateTaskAsync(json);
            if (!success)
            {
                _logger.Log(Guid.Empty, "[POST] task creation failed", 3);
                return StatusCode(500, "Failed to create task.");
            }

            _logger.Log(Guid.Empty, "[POST] task created", 1);

            return Ok();
        }
        
        [HttpGet("all")]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _tasksService.GetAllTasksAsync();

            _logger.Log(Guid.Empty, "[GET] All tasks recived", 1);

            return Ok(tasks);
        }

    

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById([FromRoute] System.Guid id)
        {
            var task = await _tasksService.GetTaskByIdAsync(id);
            if (task == null)
            {
                _logger.Log(id, "[GET] task not found", 2);
                return NotFound();
            }
            
            _logger.Log(id, "[GET] task recived", 1);

            return Ok(task);
        }

        public class IdResultsBody
        {
            public System.Text.Json.JsonElement OutputData { get; set; }
            public int? StatusId { get; set; }
        }

        [HttpPost("{id}/results")]
        public async Task<IActionResult> PostResults([FromRoute] System.Guid id, [FromBody] IdResultsBody body)
        {
            if (body == null)
            {
                _logger.Log(id, "[POST] task results submission failed", 2);
                return BadRequest();
            }
            
            var output = System.Text.Json.JsonSerializer.Serialize(body.OutputData);
            var statusId = body.StatusId ?? 3;
            var ok = await _tasksService.SubmitTaskResultsAsync(id, output, statusId);
            
            if (!ok)
            {
                _logger.Log(id, "[POST] task results submission failed", 2);
                return NotFound();
            }
            
            _logger.Log(id, "[POST] task results submitted", 1);

            return Ok();
        }

        public class IdStatusBody
        {
            public int? StatusId { get; set; }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> PatchStatus([FromRoute] System.Guid id, [FromBody] IdStatusBody body)
        {
            if (body == null || body.StatusId == null)
            {
                _logger.Log(id, "[PATCH] task status update failed", 2);
                return BadRequest();
            }
                
            var ok = await _tasksService.UpdateTaskStatusAsync(id, body.StatusId.Value);
            if (!ok)
            {
                _logger.Log(id, "[PATCH] task status update failed", 2);
                return NotFound();
            }
            
            _logger.Log(id, "[PATCH] task status updated", 1);

            return Ok();
        }
        
    }
}
