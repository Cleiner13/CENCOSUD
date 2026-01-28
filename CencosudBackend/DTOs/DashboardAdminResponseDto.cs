namespace CencosudBackend.DTOs
{
    public class DashboardAdminDetalleDto
    {
        public string Supervisor { get; set; }
        public int Wapeos { get; set; }
        public int Ventas { get; set; }
        public int? MetaWapeosEquipo { get; set; }
        public int? MetaVentasEquipo { get; set; }
    }

    public class DashboardAdminTotalesDto
    {
        public int TotalWapeos { get; set; }
        public int TotalVentas { get; set; }
    }
}
