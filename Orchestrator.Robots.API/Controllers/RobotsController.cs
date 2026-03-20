using Microsoft.AspNetCore.Mvc;
using Orchestrator.Robots.API.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using Orchestrator.Core;

namespace Orchestrator.Robots.API.Controllers
{
    [ApiController]
    [Route("api/robots")]
    public class RobotsController : ControllerBase
    {
        private readonly IRobotService _robotService;
        private readonly Orchestrator.Core.Data.AppDbContext _context;
        private readonly Logger _logger;

        public RobotsController(IRobotService robotService, Orchestrator.Core.Data.AppDbContext context)
        {
            _robotService = robotService;
            _context = context;
            _logger = new Logger(context);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var robots = _context.Robots.Join(_context.RobotStatuses,
                r => r.RobotStatusId,
                s => s.Id,
                (r, s) => new { r.Id, r.Name, apiKey = r.API_KEY, status = s.Name }).ToList();

            _logger.Log(Guid.Empty, "[GET] All tasks recived", 1);
            return Ok(robots);
        }

        public class CreateRobotBody
        {
            public string Name { get; set; }
            public string? ApiKey { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRobot([FromBody] CreateRobotBody body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.Name))
            {
                _logger.Log(Guid.Empty, "[POST] Invalid robot creation request", 2);
                return BadRequest();
            }
            var robot = await _robotService.CreateRobotAsync(body.Name, body.ApiKey);

            _logger.Log(Guid.Empty, $"[POST] Robot created with id: {robot.Id}", 1);

            return CreatedAtAction(nameof(GetAll), new { id = robot.Id }, new { robot.Id, robot.Name, apiKey = robot.API_KEY });
        }

        [HttpGet("next-task")]
        public async Task<IActionResult> NextTask([FromHeader(Name = "X-ApiKey")] string apiKey)
        {
            var robot = await _robotService.GetByApiKeyAsync(apiKey);
            if (robot == null)
            {
                _logger.Log(Guid.Empty, "[GET] Unauthorized access attempt with invalid API key", 2);
                return Unauthorized();
            }

            var task = await _robotService.GetNextTaskAsync(robot);
            if (task == null)
            {
                return NoContent();
            }

            return Ok(task);
        }
    }
}
