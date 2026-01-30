// DTOs/OcrCencosudResumenCamposDto.cs
using System.Text.Json.Serialization;

namespace CencosudBackend.DTOs
{
    public class OcrCencosudResumenCamposDto
    {
        [JsonPropertyName("dni")]
        public string? Dni { get; set; }

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("tipo_tramite")]
        public string? TipoTramite { get; set; }

        [JsonPropertyName("oferta")]
        public string? Oferta { get; set; }

        [JsonPropertyName("avance_efectivo")]
        public string? AvanceEfectivo { get; set; }

        [JsonPropertyName("incremento_de_linea")]
        public string? IncrementoDeLinea { get; set; }

        // campos dinámicos que guardarás en InfoAdicional
        [JsonPropertyName("adicionales")]
        public string? Adicionales { get; set; }

        [JsonPropertyName("efectivo_cencosud")]
        public string? EfectivoCencosud { get; set; }

        [JsonPropertyName("ec_pct")]
        public string? EcPct { get; set; }

        [JsonPropertyName("ec_tasa")]
        public string? EcTasa { get; set; }

        [JsonPropertyName("ae_pct")]
        public string? AePct { get; set; }

        [JsonPropertyName("ae_tasa")]
        public string? AeTasa { get; set; }

        // si decides usar alertas comerciales en el futuro
        [JsonPropertyName("alertas_comerciales")]
        public string? AlertasComerciales { get; set; }
    }

    public class OcrCencosudResumenResponseDto
    {
        public OcrCencosudResumenCamposDto Campos { get; set; } = new();
        public string? RawJson { get; set; }
    }
}
