using CencosudBackend.Models;

namespace CencosudBackend.Services
{
    public interface IJwtService
    {
        string GenerarToken(LoginResult usuario);
    }
}
