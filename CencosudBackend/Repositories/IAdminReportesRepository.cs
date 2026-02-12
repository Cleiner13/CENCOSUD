using CencosudBackend.DTOs;
using CencosudBackend.DTOs.AdminReportes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CencosudBackend.Repositories
{
    public interface IAdminReportesRepository
    {
        Task<AdminKpisResponseDto> ObtenerKpisAsync(DateTime fechaIni, DateTime fechaFin, string periodo, string? uunn);
        Task<List<AdminSerieDiariaItemDto>> ObtenerSerieDiariaAsync(DateTime fechaIni, DateTime fechaFin, string? supervisor, string? uunn);
        Task<List<AdminRankingSupervisorDto>> ObtenerRankingSupervisoresAsync(DateTime fechaIni, DateTime fechaFin, string periodo, string? uunn);
        Task<List<AdminTramitesMesRowDto>> ObtenerTramitesMesAsync(AdminTramitesMesRequestDto req);
        Task<List<AdminHoraWapeoRowDto>> ObtenerHoraWapeoPorDiaAsync(AdminHoraWapeoPorDiaRequestDto req);
    }
}
