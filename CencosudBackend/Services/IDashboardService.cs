using CencosudBackend.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CencosudBackend.Services
{
    public interface IDashboardService
    {
        Task<(List<DashboardAsesorResponseDto>, DashboardAsesorTotalesDto)> GetAsesorDashboardAsync(string uunn, string asesor, DateTime? fechaIni, DateTime? fechaFin);
        Task<(List<DashboardSupervisorDetalleDto>, DashboardSupervisorTotalesDto)> GetSupervisorDashboardAsync(string uunn, string supervisor, DateTime? fechaIni, DateTime? fechaFin);
        Task<(List<DashboardAdminDetalleDto>, DashboardAdminTotalesDto)> GetAdminDashboardAsync(string uunn, DateTime? fechaIni, DateTime? fechaFin);
    }
}
