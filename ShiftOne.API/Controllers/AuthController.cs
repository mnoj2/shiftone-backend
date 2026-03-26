using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftOne.Application.Dtos;
using ShiftOne.Application.Interfaces.Common;

namespace ShiftOne.API.Controllers.Auth {
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

        [HttpPost("refresh")]
        [Authorize]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken) {
            var result = await _authService.RefreshTokenAsync(refreshToken);
            if(result is null) {
                return Unauthorized(new { Message = "Refresh failed: Invalid or expired token" });
            }
            return Ok(result);
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke([FromBody] string refreshToken) {
            var result = await _authService.RevokeRefreshTokenAsync(refreshToken);
            if(!result) {
                return BadRequest(new { Message = "Revoke failed: Invalid token" });
            }
            return Ok(new { Message = "Token revoked successfully" });
        }

        [HttpPost("health")]
        public async Task<IActionResult> Health() {
            return Ok("API is running");
        }
    }
}
