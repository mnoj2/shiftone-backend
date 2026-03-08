using Microsoft.AspNetCore.Mvc;
using ShiftOne.Application.Dtos;
using ShiftOne.Application.Interfaces;

namespace ShiftOne.API.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {

        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request) {

            var result = await _authService.LoginAsync(request);
            if(result is null) {
                return Unauthorized(new { Message = "Login failed: Invalid credentials" });
            }
            return Ok(result);
        }
    }
}
