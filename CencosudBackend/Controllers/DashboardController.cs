using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CencosudBackend.Services;

namespace CencosudBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service)
        {
            _service = service;
        }

        [HttpGet("asesor")]
        [Authorize(Roles = "ASESOR,SUPERVISOR,ADMIN")]
        public async Task<IActionResult> GetAsesorDashboard([FromQuery] string uunn, [FromQuery] string asesor,
            [FromQuery] DateTime? fechaIni, [FromQuery] DateTime? fechaFin)
        {
            var result = await _service.GetAsesorDashboardAsync(uunn, asesor, fechaIni, fechaFin);
            return Ok(new { Estados = result.Item1, Totales = result.Item2 });
        }

        [HttpGet("supervisor")]
        [Authorize(Roles = "SUPERVISOR,ADMIN")]
        public async Task<IActionResult> GetSupervisorDashboard([FromQuery] string uunn, [FromQuery] string supervisor,
            [FromQuery] DateTime? fechaIni, [FromQuery] DateTime? fechaFin)
        {
            var result = await _service.GetSupervisorDashboardAsync(uunn, supervisor, fechaIni, fechaFin);
            return Ok(new { Detalle = result.Item1, Totales = result.Item2 });
        }

        [HttpGet("admin")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAdminDashboard([FromQuery] string uunn,
            [FromQuery] DateTime? fechaIni, [FromQuery] DateTime? fechaFin)
        {
            var result = await _service.GetAdminDashboardAsync(uunn, fechaIni, fechaFin);
            return Ok(new { Detalle = result.Item1, Totales = result.Item2 });
        }
    }
}
