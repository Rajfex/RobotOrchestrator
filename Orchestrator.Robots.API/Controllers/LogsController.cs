using Microsoft.AspNetCore.Http.HttpResults;
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

        [HttpGet("/api/logs")]
        public IActionResult GetLogs()
        {
            var logs = _context.Logs.ToList();
            return Ok(logs);
        }
    }
}
