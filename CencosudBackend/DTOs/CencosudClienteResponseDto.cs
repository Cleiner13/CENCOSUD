namespace CencosudBackend.DTOs
{
    public class CencosudClienteResponseDto
    {
        public int? IdCliente { get; set; }
        public int CodigoResultado { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
