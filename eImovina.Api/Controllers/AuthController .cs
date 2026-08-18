using eImovina.Api.Data;
using eImovina.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using eImovina.Shared.DTOs.Auth;

namespace eImovina.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController: ControllerBase
    {
        private readonly eImovinaDbContext _context;
        private readonly JwtTokenService _tokenService;

        public AuthController(eImovinaDbContext context, JwtTokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            // Implementation for login logic
            return Ok();
        }
    }
}
