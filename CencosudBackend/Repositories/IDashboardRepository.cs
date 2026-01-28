using CencosudBackend.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CencosudBackend.Repositories
{
    public interface IDashboardRepository
    {
        Task<(List<DashboardAsesorResponseDto> Estados, DashboardAsesorTotalesDto Totales)>
            GetDashboardAsesorAsync(string uunn, string asesor, DateTime? fechaIni, DateTime? fechaFin);

        Task<(List<DashboardSupervisorDetalleDto> Detalle, DashboardSupervisorTotalesDto Totales)>
            GetDashboardSupervisorAsync(string uunn, string supervisor, DateTime? fechaIni, DateTime? fechaFin);

        Task<(List<DashboardAdminDetalleDto> Detalle, DashboardAdminTotalesDto Totales)>
            GetDashboardAdminAsync(string uunn, DateTime? fechaIni, DateTime? fechaFin);
    }
}
