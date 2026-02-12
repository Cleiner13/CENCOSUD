using CencosudBackend.DTOs;
using CencosudBackend.DTOs.AdminReportes;
using CencosudBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CencosudBackend.DTOs.AdminReportes;

namespace CencosudBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CencosudAdminReportesController : ControllerBase
    {
        private readonly IAdminReportesService _service;

        public CencosudAdminReportesController(IAdminReportesService service)
        {
            _service = service;
        }

        // GET /api/CencosudAdminReportes/kpis?periodo=2026-01&uunn=CENCOSUD-TC
        [HttpGet("kpis")]
        public async Task<ActionResult<AdminKpisResponseDto>> GetKpis(
            [FromQuery] string periodo,
            [FromQuery] string? uunn = null)
        {
            try
            {
                var data = await _service.ObtenerKpisAsync(User, periodo, uunn);
                return Ok(data);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET /api/CencosudAdminReportes/serie-diaria?periodo=2026-01&supervisor=...&uunn=...
        [HttpGet("serie-diaria")]
        public async Task<ActionResult<List<AdminSerieDiariaItemDto>>> GetSerieDiaria(
            [FromQuery] string periodo,
            [FromQuery] string? supervisor = null,
            [FromQuery] string? uunn = null)
        {
            try
            {
                var data = await _service.ObtenerSerieDiariaAsync(User, periodo, supervisor, uunn);
                return Ok(data);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // GET /api/CencosudAdminReportes/ranking-supervisores?periodo=2026-01&uunn=...
        [HttpGet("ranking-supervisores")]
        public async Task<ActionResult<List<AdminRankingSupervisorDto>>> GetRankingSupervisores(
            [FromQuery] string periodo,
            [FromQuery] string? uunn = null)
        {
            try
            {
                var data = await _service.ObtenerRankingSupervisoresAsync(User, periodo, uunn);
                return Ok(data);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        // =========================
        // NUEVOS ENDPOINTS
        // =========================

        [HttpPost("tramites-mes")]
        public async Task<IActionResult> PostTramitesMes([FromBody] AdminTramitesMesRequestDto req)
        {
            try
            {
                var data = await _service.ObtenerTramitesMesAsync(User, req);
                return Ok(data);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("hora-wapeo-por-dia")]
        public async Task<IActionResult> PostHoraWapeoPorDia([FromBody] AdminHoraWapeoPorDiaRequestDto req)
        {
            try
            {
                var data = await _service.ObtenerHoraWapeoPorDiaAsync(User, req);
                return Ok(data);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
