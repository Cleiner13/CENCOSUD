using System.Security.Claims;
using CencosudBackend.DTOs;

namespace CencosudBackend.Services
{
    public interface IMetasService
    {
        Task<List<MetaAsesorDto>> ObtenerMetasAsesoresAsync(ClaimsPrincipal user, int periodo, string? uunn);
        Task GuardarMetaAsesorAsync(ClaimsPrincipal user, MetaAsesorDto dto, string? uunn);

        Task<List<MetaSupervisorDto>> ObtenerMetasSupervisoresAsync(ClaimsPrincipal user, int periodo, string uunn);
        Task GuardarMetaSupervisorAsync(ClaimsPrincipal user, MetaSupervisorDto dto, string uunn);
    }
}
