using System.Threading.Tasks;
using CencosudBackend.DTOs;

namespace CencosudBackend.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    }
}
