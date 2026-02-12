namespace CencosudBackend.DTOs.AdminReportes
{
    public class AdminHoraWapeoPorDiaRequestDto
    {
        public DateTime FechaIni { get; set; }
        public DateTime FechaFin { get; set; }
        public string Supervisor { get; set; } = "";
        public string? Asesor { get; set; }
        public string? Uunn { get; set; }
    }
}
