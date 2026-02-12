namespace CencosudBackend.DTOs
{
    public class AdminTramitesMesRequestDto
    {
        public DateTime FechaIni { get; set; }
        public DateTime FechaFin { get; set; }
        /// <summary>Puede ser 'TODO' o 'VENTAS'</summary>
        public string FiltroTipo { get; set; } = "TODO";
        public string? Uunn { get; set; }
    }
}
