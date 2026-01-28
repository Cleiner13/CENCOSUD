using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CencosudBackend.DTOs;
using CencosudBackend.Services;
using CencosudBackend.Repositories;

namespace CencosudBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CencosudReportesController : ControllerBase
    {
        private readonly ICencosudTiendaService _service;
        private readonly IMetasRepository _metasRepository; // ✅ NUEVO

        public CencosudReportesController(
            ICencosudTiendaService service,
            IMetasRepository metasRepository) // ✅ NUEVO
        {
            _service = service;
            _metasRepository = metasRepository;
        }

        [HttpGet("resumen-mensual")]
        public async Task<ActionResult<List<CencosudResumenMensualDto>>> GetResumenMensual(
            [FromQuery] DateTime? fechaIni,
            [FromQuery] DateTime? fechaFin)
        {
            var data = await _service.ObtenerResumenMensualAsync(User, fechaIni, fechaFin);
            return Ok(data);
        }

        // ✅ NUEVO: lista asesores del supervisor logueado
        // GET: /api/CencosudReportes/asesores?uunn=CENCOSUD-TC
        [HttpGet("asesores")]
        public async Task<ActionResult<List<string>>> GetAsesores([FromQuery] string? uunn)
        {
            var rolApp = (User.FindFirst(ClaimTypes.Role)?.Value ?? "ASESOR").Trim().ToUpper();

            // solo supervisor/admin necesitan esto
            if (rolApp != "SUPERVISOR" && rolApp != "ADMIN")
                return Ok(new List<string>());

            var supervisor =
                User.Identity?.Name ??
                User.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(supervisor))
                return Ok(new List<string>());

            var asesores = await _metasRepository.ListarAsesoresPorSupervisorAsync(supervisor, uunn);
            return Ok(asesores);
        }

        // ✅ LISTADO con vendedor
        // GET: /api/CencosudReportes/listado?...&vendedor=USUARIO
        [HttpGet("listado")]
        public async Task<ActionResult<CencosudVentasListadoResponseDto>> GetListado(
            [FromQuery] DateTime? fechaIni,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] string? estado,
            [FromQuery] string? dniCliente,
            [FromQuery] string? vendedor, // ✅ NUEVO
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var data = await _service.ObtenerListadoVentasAsync(
                User,
                fechaIni,
                fechaFin,
                estado,
                dniCliente,
                vendedor, // ✅
                page,
                pageSize);

            return Ok(data);
        }

        // ✅ EXCEL con vendedor (para descargar filtrado)
        [HttpGet("listado-excel")]
        public async Task<IActionResult> DescargarListadoExcel(
            [FromQuery] DateTime? fechaIni,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] string? estado,
            [FromQuery] string? dniCliente,
            [FromQuery] string? vendedor) // ✅ NUEVO
        {
            var bytes = await _service.GenerarExcelListadoVentasAsync(
                User,
                fechaIni,
                fechaFin,
                estado,
                dniCliente,
                vendedor);

            var nombreArchivo =
                $"ventas_cencosud_{fechaIni:yyyyMMdd}_{fechaFin:yyyyMMdd}.xlsx";

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                nombreArchivo
            );
        }
    }
}
