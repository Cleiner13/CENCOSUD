using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CencosudBackend.DTOs;
using CencosudBackend.Repositories;

namespace CencosudBackend.Services
{
    public class MetasService : IMetasService
    {
        private readonly IMetasRepository _repo;

        public MetasService(IMetasRepository repo)
        {
            _repo = repo;
        }

        private static (string RolApp, string Usuario, string Uunn) ExtraerDatosUsuario(ClaimsPrincipal user)
        {
            var rolApp = user.FindFirst(ClaimTypes.Role)?.Value ?? "ASESOR";

            var usuario =
                user.Identity?.Name ??
                user.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ??
                string.Empty;

            var uunn = user.FindFirst("uunn")?.Value ?? string.Empty;

            return (rolApp, usuario, uunn);
        }

        private static void ValidarPeriodo(int periodo)
        {
            if (periodo < 200001 || periodo > 209912)
                throw new ArgumentException("Periodo inválido. Usa formato YYYYMM.");
        }

        private static void ValidarMetas(int? w, int? v)
        {
            if (w is < 0 || v is < 0)
                throw new ArgumentException("Las metas no pueden ser negativas.");
        }

        // ==================== SUPERVISOR: METAS POR ASESOR ====================

        public async Task<List<MetaAsesorDto>> ObtenerMetasAsesoresAsync(ClaimsPrincipal user, int periodo, string? uunn)
        {
            var (rol, supervisor, uunnClaim) = ExtraerDatosUsuario(user);

            if (rol != "SUPERVISOR")
                throw new UnauthorizedAccessException("Solo SUPERVISOR puede ver metas de asesores.");

            ValidarPeriodo(periodo);

            var uunnFinal = string.IsNullOrWhiteSpace(uunn) ? uunnClaim : uunn;

            // 1) lista completa de asesores del supervisor
            var asesores = await _repo.ListarAsesoresPorSupervisorAsync(supervisor, uunnFinal);

            // 2) metas existentes
            var metas = await _repo.ListarMetasAsesoresAsync(uunnFinal, periodo, supervisor);
            var dict = metas.ToDictionary(x => x.Asesor, StringComparer.OrdinalIgnoreCase);

            // 3) merge: devuelve TODOS, con o sin meta
            var result = new List<MetaAsesorDto>();
            foreach (var a in asesores)
            {
                if (dict.TryGetValue(a, out var m))
                {
                    result.Add(m);
                }
                else
                {
                    result.Add(new MetaAsesorDto
                    {
                        Uunn = uunnFinal,
                        Periodo = periodo,
                        Supervisor = supervisor,
                        Asesor = a,
                        MetaWapeos = null,
                        MetaVentas = null,
                        UsuarioAccion = string.Empty
                    });
                }
            }

            return result.OrderBy(x => x.Asesor).ToList();
        }

        public async Task GuardarMetaAsesorAsync(ClaimsPrincipal user, MetaAsesorDto dto, string? uunn)
        {
            var (rol, supervisor, uunnClaim) = ExtraerDatosUsuario(user);

            if (rol != "SUPERVISOR")
                throw new UnauthorizedAccessException("Solo SUPERVISOR puede asignar metas a asesores.");

            ValidarPeriodo(dto.Periodo);
            ValidarMetas(dto.MetaWapeos, dto.MetaVentas);

            var uunnFinal = string.IsNullOrWhiteSpace(uunn) ? uunnClaim : uunn;

            // ⚠️ Seguridad: ignoramos lo que venga desde frontend en Supervisor/UsuarioAccion/Uunn
            dto.Uunn = uunnFinal;
            dto.Supervisor = supervisor;
            dto.UsuarioAccion = supervisor;

            await _repo.UpsertMetaAsesorAsync(dto);
        }

        // ==================== ADMIN: METAS POR SUPERVISOR ====================

        public async Task<List<MetaSupervisorDto>> ObtenerMetasSupervisoresAsync(ClaimsPrincipal user, int periodo, string uunn)
        {
            var (rol, usuarioAccion, _) = ExtraerDatosUsuario(user);

            if (rol != "ADMIN")
                throw new UnauthorizedAccessException("Solo ADMIN puede ver metas de supervisores.");

            ValidarPeriodo(periodo);

            // 1) todos los supervisores activos
            var supervisores = await _repo.ListarSupervisoresPorUunnAsync(uunn);

            // 2) metas existentes
            var metas = await _repo.ListarMetasSupervisoresAsync(uunn, periodo);
            var dict = metas.ToDictionary(x => x.Supervisor, StringComparer.OrdinalIgnoreCase);

            // 3) merge
            var result = new List<MetaSupervisorDto>();
            foreach (var s in supervisores)
            {
                if (dict.TryGetValue(s, out var m))
                {
                    result.Add(m);
                }
                else
                {
                    result.Add(new MetaSupervisorDto
                    {
                        Uunn = uunn,
                        Periodo = periodo,
                        Supervisor = s,
                        MetaWapeos = null,
                        MetaVentas = null,
                        UsuarioAccion = string.Empty
                    });
                }
            }

            return result.OrderBy(x => x.Supervisor).ToList();
        }

        public async Task GuardarMetaSupervisorAsync(ClaimsPrincipal user, MetaSupervisorDto dto, string uunn)
        {
            var (rol, usuarioAccion, _) = ExtraerDatosUsuario(user);

            if (rol != "ADMIN")
                throw new UnauthorizedAccessException("Solo ADMIN puede asignar metas a supervisores.");

            ValidarPeriodo(dto.Periodo);
            ValidarMetas(dto.MetaWapeos, dto.MetaVentas);

            dto.Uunn = uunn;
            dto.UsuarioAccion = usuarioAccion;

            await _repo.UpsertMetaSupervisorAsync(dto);
        }
    }
}
