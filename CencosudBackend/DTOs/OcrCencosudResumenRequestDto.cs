using Microsoft.AspNetCore.Http;

namespace CencosudBackend.DTOs
{
    public class OcrCencosudResumenRequestDto
    {
        // IMPORTANT: el nombre del campo en el form-data debe ser "imagen"
        public IFormFile Imagen { get; set; } = default!;
    }
}
