using System.Threading.Tasks;
using CencosudBackend.DTOs;
using CencosudBackend.Helpers;
using CencosudBackend.Repositories;

namespace CencosudBackend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IJwtService _jwtService;

        public AuthService(IAuthRepository authRepository, IJwtService jwtService)
        {
            _authRepository = authRepository;
            _jwtService = jwtService;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var response = new LoginResponseDto();

            // 1. Hashear contraseña igual que en Python
            var passwordHash = PasswordEncoder.EncodePassword(request.Password);

            // 2. Llamar al SP
            var loginResult = await _authRepository.ValidarLoginAsync(request.Usuario, passwordHash);

            response.CodigoResultado = loginResult.CodigoResultado;
            response.Mensaje = loginResult.Mensaje;

            if (loginResult.CodigoResultado != 0)
            {
                // Usuario/contraseña incorrectos
                return response;
            }

            // 3. Generar token JWT
            var token = _jwtService.GenerarToken(loginResult);
            response.Token = token;

            response.IdUsuario = loginResult.IdUsuario;
            response.Usuario = loginResult.Usuario;
            response.Estado = loginResult.Estado;
            response.Cargo = loginResult.Cargo;
            response.Supervisor = loginResult.Supervisor;
            response.UUNN = loginResult.UUNN;
            response.RolApp = loginResult.RolApp;

            return response;
        }
    }
}
