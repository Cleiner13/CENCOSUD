using CencosudBackend.DTOs;
using CencosudBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CencosudBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OcrController : ControllerBase
    {
        private readonly IOpenAiVisionService _visionService;

        public OcrController(IOpenAiVisionService visionService)
        {
            _visionService = visionService;
        }

        /// <summary>
        /// Lee la imagen con GPT-4o (visión) y devuelve los campos del RESUMEN.
        /// </summary>
        /// <remarks>
        /// Enviar como multipart/form-data con un único campo "imagen".
        /// </remarks>
        [HttpPost("cencosud-resumen-ia")]
  
        public async Task<ActionResult<OcrCencosudResumenResponseDto>> LeerResumenConIa(
            [FromForm] OcrCencosudResumenRequestDto request)
        {
            if (request.Imagen == null || request.Imagen.Length == 0)
                return BadRequest("Debe enviar una imagen en el campo 'imagen'.");

            await using var ms = new MemoryStream();
            await request.Imagen.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var (campos, rawJson) =
                await _visionService.LeerResumenAsync(bytes, request.Imagen.ContentType ?? "image/jpeg");

            var response = new OcrCencosudResumenResponseDto
            {
                Campos = campos,
                RawJson = rawJson
            };

            return Ok(response);
        }
    }
}
