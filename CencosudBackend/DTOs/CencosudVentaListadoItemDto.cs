using System;

namespace CencosudBackend.DTOs
{
    public class CencosudVentaListadoItemDto
    {

        public int IdCliente { get; set; }
        public string DniCliente { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? Comentario { get; set; }

        public string? UsuarioCencosud { get; set; }
        public string? Cargo { get; set; }
        public string? Supervisor { get; set; }
        public string? Uunn { get; set; }
        public string? NomVendedor { get; set; }

        public DateTime DateEntry { get; set; }
        public bool TieneImagen { get; set; }

        // Si después quieres mostrar campos dinámicos (Oferta, Avance, etc)
        // podemos añadir aquí propiedades extra o un Dictionary<string,string>.
    }
}
