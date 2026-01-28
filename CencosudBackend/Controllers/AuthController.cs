using System.Threading.Tasks;
using CencosudBackend.DTOs;
using CencosudBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CencosudBackend.Controllers
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

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { Mensaje = "Usuario y contraseña son obligatorios." });

            var result = await _authService.LoginAsync(request);

            if (result.CodigoResultado != 0)
                return Unauthorized(result);

            return Ok(result);
        }
    }
}
