using Microsoft.AspNetCore.Mvc;
using Orchestrator.Core.Data;

namespace Orchestrator.Robots.API.Controllers
{
    public class LogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LogsController(AppDbContext context)
        { 
            _context = context;
        }

        [HttpGet("/api/logs/{pageNumber:int}")]
        public IActionResult GetLogs(int pageNumber)
        {
            if (pageNumber < 1)
            {
                return BadRequest("Page number must be higher than 0.");
            }

            const int pageSize = 50;

            var logs = _context.Logs
                .OrderByDescending(l => l.Created)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(logs);
        }
    }
}
