using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CencosudBackend.DTOs;
using CencosudBackend.Services;

namespace CencosudBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MetasController : ControllerBase
    {
        private readonly IMetasService _service;

        public MetasController(IMetasService service)
        {
            _service = service;
        }

        // SUPERVISOR: ver lista de asesores con metas (o sin meta)
        // GET /api/Metas/asesores?periodo=202512&uunn=CENCOSUD%20TC (uunn opcional)
        [HttpGet("asesores")]
        [Authorize(Roles = "SUPERVISOR")]
        public async Task<IActionResult> GetMetasAsesores([FromQuery] int periodo, [FromQuery] string? uunn = null)
        {
            var data = await _service.ObtenerMetasAsesoresAsync(User, periodo, uunn);
            return Ok(data);
        }

        // SUPERVISOR: asignar/quitar meta a un asesor
        // POST /api/Metas/asesor?uunn=CENCOSUD%20TC (uunn opcional)
        [HttpPost("asesor")]
        [Authorize(Roles = "SUPERVISOR")]
        public async Task<IActionResult> UpsertMetaAsesor([FromBody] MetaAsesorDto dto, [FromQuery] string? uunn = null)
        {
            await _service.GuardarMetaAsesorAsync(User, dto, uunn);
            return Ok(new { ok = true, mensaje = "Meta de asesor actualizada." });
        }

        // ADMIN: ver lista de supervisores con metas (o sin meta)
        // GET /api/Metas/supervisores?periodo=202512&uunn=CENCOSUD%20TC
        [HttpGet("supervisores")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetMetasSupervisores([FromQuery] int periodo, [FromQuery] string uunn)
        {
            var data = await _service.ObtenerMetasSupervisoresAsync(User, periodo, uunn);
            return Ok(data);
        }

        // ADMIN: asignar/quitar meta a un supervisor
        // POST /api/Metas/supervisor?uunn=CENCOSUD%20TC
        [HttpPost("supervisor")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpsertMetaSupervisor([FromBody] MetaSupervisorDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Uunn))
                return BadRequest(new { message = "UUNN es requerido." });

            await _service.GuardarMetaSupervisorAsync(User, dto, dto.Uunn);
            return Ok(new { ok = true, mensaje = "Meta de supervisor actualizada." });
        }
    }
}
