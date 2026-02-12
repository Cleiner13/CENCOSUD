using CencosudBackend.DTOs;
using CencosudBackend.DTOs.AdminReportes;
using CencosudBackend.Repositories;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CencosudBackend.Services
{
    public class AdminReportesService : IAdminReportesService
    {
        private readonly IAdminReportesRepository _repo;

        public AdminReportesService(IAdminReportesRepository repo)
        {
            _repo = repo;
        }

        // =========================
        // NUEVOS REPORTES
        // =========================
        public Task<List<AdminTramitesMesRowDto>> ObtenerTramitesMesAsync(ClaimsPrincipal user, AdminTramitesMesRequestDto req)
        {
            ValidarAdmin(user);
            NormalizarRequestTramites(req);
            return _repo.ObtenerTramitesMesAsync(req);
        }

        public Task<List<AdminHoraWapeoRowDto>> ObtenerHoraWapeoPorDiaAsync(ClaimsPrincipal user, AdminHoraWapeoPorDiaRequestDto req)
        {
            ValidarAdmin(user);
            NormalizarRequestHoraWapeo(req);
            return _repo.ObtenerHoraWapeoPorDiaAsync(req);
        }

        // =========================
        // YA EXISTENTES
        // =========================
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

        // =========================
        // Helpers
        // =========================
        private static (string RolApp, string Usuario) ExtraerDatosUsuario(ClaimsPrincipal user)
        {
            var rolApp = user.FindFirst(ClaimTypes.Role)?.Value ?? "ASESOR";
            var usuario =
                user.Identity?.Name ??
                user.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ??
                string.Empty;

            return (rolApp.Trim().ToUpperInvariant(), usuario);
        }

        private static void ValidarAdmin(ClaimsPrincipal user)
        {
            var (rolApp, _) = ExtraerDatosUsuario(user);
            if (rolApp != "ADMIN")
                throw new UnauthorizedAccessException("Solo ADMIN puede acceder a este reporte.");
        }

        private static (DateTime ini, DateTime fin, string periodoNorm) ParsePeriodo(string periodo)
        {
            // periodo esperado: "YYYY-MM"
            var parts = periodo.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                throw new ArgumentException("Periodo inválido. Usa formato YYYY-MM.");

            var y = int.Parse(parts[0]);
            var m = int.Parse(parts[1]);

            var ini = new DateTime(y, m, 1);
            var fin = ini.AddMonths(1).AddDays(-1);

            return (ini, fin, $"{y:D4}-{m:D2}");
        }

        private static void NormalizarRequestTramites(AdminTramitesMesRequestDto req)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));

            // Si llega invertido, lo corregimos
            if (req.FechaFin < req.FechaIni)
            {
                var tmp = req.FechaIni;
                req.FechaIni = req.FechaFin;
                req.FechaFin = tmp;
            }

            req.FiltroTipo = (req.FiltroTipo ?? "TODO").Trim().ToUpperInvariant();
            if (req.FiltroTipo != "TODO" && req.FiltroTipo != "VENTAS")
                req.FiltroTipo = "TODO";

            if (string.IsNullOrWhiteSpace(req.Uunn))
                req.Uunn = null;
        }

        private static void NormalizarRequestHoraWapeo(AdminHoraWapeoPorDiaRequestDto req)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));

            if (req.FechaFin < req.FechaIni)
            {
                var tmp = req.FechaIni;
                req.FechaIni = req.FechaFin;
                req.FechaFin = tmp;
            }

            // En tu SP: Supervisor puede ser NULL para "todos" (según tu diseño)
            // Si llega vacío, lo pasamos a NULL para que funcione como "TODOS"
            if (string.IsNullOrWhiteSpace(req.Supervisor))
                req.Supervisor = null;
            else
                req.Supervisor = req.Supervisor.Trim();

            if (string.IsNullOrWhiteSpace(req.Asesor))
                req.Asesor = null;

            if (string.IsNullOrWhiteSpace(req.Uunn))
                req.Uunn = null;
        }
    }
}
