using System.Collections.Generic;

namespace CencosudBackend.DTOs
{
    public class CencosudVentasListadoResponseDto
    {
        public int TotalFilas { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public List<CencosudVentaListadoItemDto> Registros { get; set; } = new();
    }
}
