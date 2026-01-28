namespace CencosudBackend.DTOs
{
    public class DashboardAsesorResponseDto
    {
        public string Estado { get; set; }
        public int Cantidad { get; set; }
    }

    public class DashboardAsesorTotalesDto
    {
        public int TotalWapeos { get; set; }
        public int TotalVentas { get; set; }
        public int? MetaWapeos { get; set; }
        public int? MetaVentas { get; set; }
        public decimal? AvanceWapeosPct { get; set; }
        public decimal? AvanceVentasPct { get; set; }
    }
}
