using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using CencosudBackend.DTOs;
using CencosudBackend.Repositories;

namespace CencosudBackend.Services
{
    public class AdminReportesService : IAdminReportesService
    {
        private readonly IAdminReportesRepository _repo;

        public AdminReportesService(IAdminReportesRepository repo)
        {
            _repo = repo;
        }

        private static (string RolApp, string Usuario) ExtraerDatosUsuario(ClaimsPrincipal user)
        {
            var rolApp = user.FindFirst(ClaimTypes.Role)?.Value ?? "ASESOR";
            var usuario =
                user.Identity?.Name ??
                user.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ??
                string.Empty;

            return (rolApp.Trim().ToUpper(), usuario);
        }

        private static (DateTime ini, DateTime fin, string periodoNorm) ParsePeriodo(string periodo)
        {
            // periodo esperado: "YYYY-MM"
            // ini: primer día del mes, fin: último día del mes
            var parts = periodo.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) throw new ArgumentException("Periodo inválido. Usa formato YYYY-MM.");

            var y = int.Parse(parts[0]);
            var m = int.Parse(parts[1]);

            var ini = new DateTime(y, m, 1);
            var fin = ini.AddMonths(1).AddDays(-1);

            return (ini, fin, $"{y:D4}-{m:D2}");
        }

        private static void ValidarAdmin(ClaimsPrincipal user)
        {
            var (rolApp, _) = ExtraerDatosUsuario(user);
            if (rolApp != "ADMIN")
                throw new UnauthorizedAccessException("Solo ADMIN puede acceder a este reporte.");
        }

        public async Task<AdminKpisResponseDto> ObtenerKpisAsync(ClaimsPrincipal user, string periodo, string? uunn)
        {
            ValidarAdmin(user);

            var (ini, fin, periodoNorm) = ParsePeriodo(periodo);
            return await _repo.ObtenerKpisAsync(ini, fin, periodoNorm, uunn);
        }

        public async Task<List<AdminSerieDiariaItemDto>> ObtenerSerieDiariaAsync(ClaimsPrincipal user, string periodo, string? supervisor, string? uunn)
        {
            ValidarAdmin(user);

            var (ini, fin, _) = ParsePeriodo(periodo);
            return await _repo.ObtenerSerieDiariaAsync(ini, fin, supervisor, uunn);
        }

        public async Task<List<AdminRankingSupervisorDto>> ObtenerRankingSupervisoresAsync(ClaimsPrincipal user, string periodo, string? uunn)
        {
            ValidarAdmin(user);

            var (ini, fin, periodoNorm) = ParsePeriodo(periodo);
            return await _repo.ObtenerRankingSupervisoresAsync(ini, fin, periodoNorm, uunn);
        }
    }
}
