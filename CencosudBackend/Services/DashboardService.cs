using CencosudBackend.DTOs;
using CencosudBackend.Repositories;

namespace CencosudBackend.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _repository;

        public DashboardService(IDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<(List<DashboardAsesorResponseDto>, DashboardAsesorTotalesDto)> GetAsesorDashboardAsync(string uunn, string asesor, DateTime? fechaIni, DateTime? fechaFin)
            => await _repository.GetDashboardAsesorAsync(uunn, asesor, fechaIni, fechaFin);

        public async Task<(List<DashboardSupervisorDetalleDto>, DashboardSupervisorTotalesDto)> GetSupervisorDashboardAsync(string uunn, string supervisor, DateTime? fechaIni, DateTime? fechaFin)
            => await _repository.GetDashboardSupervisorAsync(uunn, supervisor, fechaIni, fechaFin);

        public async Task<(List<DashboardAdminDetalleDto>, DashboardAdminTotalesDto)> GetAdminDashboardAsync(string uunn, DateTime? fechaIni, DateTime? fechaFin)
            => await _repository.GetDashboardAdminAsync(uunn, fechaIni, fechaFin);
    }
}
