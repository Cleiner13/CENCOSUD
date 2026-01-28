namespace CencosudBackend.DTOs
{
    public class LoginResponseDto
    {
        public int CodigoResultado { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string? Token { get; set; }

        public int? IdUsuario { get; set; }
        public string? Usuario { get; set; }
        public string? Estado { get; set; }
        public string? Cargo { get; set; }
        public string? Supervisor { get; set; }
        public string? UUNN { get; set; }
        public string? RolApp { get; set; }
    }
}
