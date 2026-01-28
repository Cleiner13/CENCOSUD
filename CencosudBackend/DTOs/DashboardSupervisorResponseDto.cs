namespace CencosudBackend.DTOs
{
    public class DashboardSupervisorDetalleDto
    {
        public string Asesor { get; set; }
        public int Wapeos { get; set; }
        public int Ventas { get; set; }
        public int? MetaWapeos { get; set; }
        public int? MetaVentas { get; set; }
    }

    public class DashboardSupervisorTotalesDto
    {
        public int TotalWapeos { get; set; }
        public int TotalVentas { get; set; }
        public int? MetaWapeosEquipo { get; set; }
        public int? MetaVentasEquipo { get; set; }
    }
}
