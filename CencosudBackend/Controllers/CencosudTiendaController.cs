// CencosudTiendaController.cs
using CencosudBackend.DTOs;
using CencosudBackend.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CencosudTiendaController : ControllerBase
{
    private readonly ICencosudTiendaService _service;

    public CencosudTiendaController(ICencosudTiendaService service)
    {
        _service = service;
    }

    [HttpPost("guardar")]
    public async Task<IActionResult> GuardarCliente([FromBody] CencosudClienteRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.GuardarClienteAsync(dto, User);
        return Ok(result);
    }

    // NUEVO: detalle para editar
    [HttpGet("cliente/{idCliente:int}")]
    public async Task<ActionResult<CencosudClienteDetalleDto>> GetClientePorId(int idCliente)
    {
        var dto = await _service.ObtenerClienteDetalleAsync(idCliente);
        if (dto is null) return NotFound();
        return Ok(dto);
    }

    // DELETE: /api/CencosudTienda/cliente/14
    [HttpDelete("cliente/{idCliente:int}")]
    public async Task<IActionResult> EliminarCliente(int idCliente)
    {
        var result = await _service.EliminarClienteAsync(idCliente);

        // Si todo bien
        if (result.CodigoResultado == 0)
            return Ok(result);

        // Si hubo error de negocio o SQL, devuelvo 400 con el mensaje del SP
        return BadRequest(result);
    }

}
