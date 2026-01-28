using System.Text.Json.Serialization;

namespace CencosudBackend.DTOs
{
    // Campos que realmente te interesan del cuadro RESUMEN
    public class OcrCencosudResumenCamposDto
    {
        [JsonPropertyName("fecha")]
        public string? Fecha { get; set; }

        [JsonPropertyName("tipo_doc")]
        public string? TipoDoc { get; set; }

        [JsonPropertyName("doc")]
        public string? Doc { get; set; }

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("tipo_de_tramite")]
        public string? TipoDeTramite { get; set; }

        [JsonPropertyName("oferta")]
        public string? Oferta { get; set; }

        [JsonPropertyName("incremento_de_linea")]
        public string? IncrementoDeLinea { get; set; }

        [JsonPropertyName("superavance")]
        public string? Superavance { get; set; }

        [JsonPropertyName("superavance_plus")]
        public string? SuperavancePlus { get; set; }

        [JsonPropertyName("avance_efectivo")]
        public string? AvanceEfectivo { get; set; }

        [JsonPropertyName("cambio_de_producto")]
        public string? CambioDeProducto { get; set; }
    }

    // Lo que devolverá el endpoint al frontend
    public class OcrCencosudResumenResponseDto
    {
        public OcrCencosudResumenCamposDto Campos { get; set; } = new();

        /// <summary>
        /// JSON crudo que devolvió el modelo (útil para debug).
        /// </summary>
        public string? RawJson { get; set; }
    }
}
