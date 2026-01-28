using System.Collections.Generic;

namespace CencosudBackend.DTOs
{
    public class CencosudClienteRequestDto
    {
        public int? IdCliente { get; set; }          // null = nuevo, valor = editar

        // Datos fijos
        public string DniCliente { get; set; } = null!;
        public string NombreCliente { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string? Estado { get; set; }
        public string? Comentario { get; set; }
        public string? UsuarioCencosud { get; set; }
        public string? Cargo { get; set; }
        public string? Supervisor { get; set; }
        public string? Uunn { get; set; }
        public string? NomVendedor { get; set; }

        // Campos dinámicos desde OCR (key = nombre de columna, value = valor)
        public Dictionary<string, string>? InfoAdicional { get; set; }

        // Imagen (por ahora solo si tiene o no)
        public bool TieneImagen { get; set; }
    }
}
