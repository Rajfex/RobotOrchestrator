using Microsoft.AspNetCore.Mvc;
using Orchestrator.Core;
using Orchestrator.UI.API.Interfaces;

namespace Orchestrator.UI.API.Controllers
{
    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private IAccountService _accountService;
        private Logger _logger;
        public AccountController(IAccountService accountService, Logger logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                _logger.Log(Guid.Empty, "[POST] Missing username or password", 3);
                return BadRequest("Username and password are required.");
            }

            var succes = await _accountService.RegisterAsync(request.Username, request.Password);
            _logger.Log(Guid.Empty, $"[POST] Registering user {request.Username}", 1);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                _logger.Log(Guid.Empty, "[POST] Missing username or password", 3);
                return BadRequest("Username and password are required.");
            }

            var token = await _accountService.LoginAsync(request.Username, request.Password);
            if (token == null)
            {
                _logger.Log(Guid.Empty, $"[POST] Failed login attempt", 3);
                return Unauthorized();
            }
            _logger.Log(Guid.Empty, $"[POST] User {request.Username} logged in", 1);
            return Ok(new { token = token });
        }
    }
}
