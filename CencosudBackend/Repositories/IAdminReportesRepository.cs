using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CencosudBackend.DTOs;

namespace CencosudBackend.Repositories
{
    public interface IAdminReportesRepository
    {
        Task<AdminKpisResponseDto> ObtenerKpisAsync(DateTime fechaIni, DateTime fechaFin, string periodo, string? uunn);
        Task<List<AdminSerieDiariaItemDto>> ObtenerSerieDiariaAsync(DateTime fechaIni, DateTime fechaFin, string? supervisor, string? uunn);
        Task<List<AdminRankingSupervisorDto>> ObtenerRankingSupervisoresAsync(DateTime fechaIni, DateTime fechaFin, string periodo, string? uunn);
    }
}
