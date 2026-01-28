namespace CencosudBackend.DTOs
{
    public class AdminRankingSupervisorDto
    {
        public string Supervisor { get; set; } = string.Empty;
        public int Asesores { get; set; }

        public int Ventas { get; set; }
        public int MetaVentas { get; set; }
        public decimal CumplimientoPct { get; set; }
    }
}
