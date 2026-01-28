namespace CencosudBackend.DTOs
{
    public class DashboardTotalesDto
    {
        public int TotalWapeos { get; set; }
        public int TotalVentas { get; set; }

        // Meta individual (ASESOR) o meta por asesor (cuando se vea detalle)
        public int? MetaWapeos { get; set; }
        public int? MetaVentas { get; set; }

        // Meta equipo (SUPERVISOR o ADMIN)
        public int? MetaWapeosEquipo { get; set; }
        public int? MetaVentasEquipo { get; set; }

        // Por si quieres mostrar % avance en UI
        public decimal? AvanceWapeosPct { get; set; }
        public decimal? AvanceVentasPct { get; set; }
    }
}
