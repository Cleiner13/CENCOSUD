// DTOs/CencosudClienteDetalleDto.cs
using System;

namespace CencosudBackend.DTOs
{
    public class CencosudClienteDetalleDto
    {
        public int IdCliente { get; set; }
        public string DniCliente { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;

        public string? Estado { get; set; }
        public string? Comentario { get; set; }
        public string? UsuarioCencosud { get; set; }
        public string? Cargo { get; set; }
        public string? Supervisor { get; set; }
        public string? Uunn { get; set; }
        public string? NomVendedor { get; set; }

        public DateTime? DateEntry { get; set; }
        public DateTime? DateModify { get; set; }

        // Campos dinámicos de INFORMACION
        public string? AvanceEfectivo { get; set; }
        public string? TipoTramite { get; set; }
        public string? Oferta { get; set; }
        public string? Superavance { get; set; }
        public string? CambioProducto { get; set; }
        public string? IncrementoLinea { get; set; }
    }
}
