using FileCloud.Contracts.Responses;
using FileCloud.Core.Abstractions;
using FileCloud.Core.Contracts.Requests;
using FileCloud.Core.Models;
using FileCloud.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileCloud.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("Get")]
        public async Task<ActionResult<List<UserResponse>>> Get()
        {
            var result = await _authService.GetUsers();
            if (!result.IsSuccess)
                return BadRequest();

            List<UserResponse> response = new List<UserResponse>();
            foreach (var user in result.Value)
            {
                if(user == null) continue;
                response.Add(new UserResponse
                    (
                        user.Id,
                        user.Login,
                        user.Email,
                        user.CreatedAt
                    ));
            }

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.IsSuccess)
            {
                return Unauthorized(result.Error);
            }

            return Ok(result.Value);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (!result.IsSuccess)
            {
                return Conflict(result.Error);
            }

            return Ok(result.Value);
        }

        [HttpPost("EndSession")]
        [Authorize]
        public async Task EndSession()
        {
            await _authService.EndSession();
        }

        [HttpDelete("Delete")]
        [Authorize]
        public async Task<IActionResult> Delete()
        {
            var result = await _authService.DeleteAsync();

            if(result.IsSuccess)
                return Ok();
            
            return BadRequest(result.Error);
        }
    }
}
