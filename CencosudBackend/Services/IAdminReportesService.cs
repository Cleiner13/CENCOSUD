using CencosudBackend.DTOs;
using CencosudBackend.DTOs.AdminReportes;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CencosudBackend.Services
{
    public interface IAdminReportesService
    {
        Task<AdminKpisResponseDto> ObtenerKpisAsync(ClaimsPrincipal user, string periodo, string? uunn);
        Task<List<AdminSerieDiariaItemDto>> ObtenerSerieDiariaAsync(ClaimsPrincipal user, string periodo, string? supervisor, string? uunn);
        Task<List<AdminRankingSupervisorDto>> ObtenerRankingSupervisoresAsync(ClaimsPrincipal user, string periodo, string? uunn);
        Task<List<AdminTramitesMesRowDto>> ObtenerTramitesMesAsync(ClaimsPrincipal user, AdminTramitesMesRequestDto req);
        Task<List<AdminHoraWapeoRowDto>> ObtenerHoraWapeoPorDiaAsync(ClaimsPrincipal user, AdminHoraWapeoPorDiaRequestDto req);
    }
}
