namespace CencosudBackend.DTOs
{
    public class AdminKpisResponseDto
    {
        public string Periodo { get; set; } = string.Empty;   // "2026-01"

        public int Ventas { get; set; }
        public int Coordinado { get; set; }
        public int SinOferta { get; set; }

        public int MetaEquipoVentas { get; set; }
        public decimal CumplimientoVentasPct { get; set; }

        public int MetaEquipoWapeos { get; set; }
        public decimal CumplimientoWapeosPct { get; set; }
    }
}
