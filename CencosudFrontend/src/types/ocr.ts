// src/types/ocr.ts

export interface OcrCencosudCampos {
  // Datos extraídos de la cabecera
  dni: string | null;
  nombre: string | null;
  tipo_tramite: string | null;
  oferta: string | null;

  // Montos principales
  avance_efectivo: string | null;
  incremento_de_linea: string | null;

  // Campos dinámicos del nuevo formato
  adicionales: string | null;
  efectivo_cencosud: string | null;
  ec_pct: string | null;
  ec_tasa: string | null;
  ae_pct: string | null;
  ae_tasa: string | null;

  // (Opcional) texto de alertas comerciales si decides usarlo más adelante
  alertas_comerciales: string | null;
}

export interface OcrCencosudResponse {
  campos: OcrCencosudCampos;
  rawJson: string; // JSON crudo devuelto por el modelo, útil para depuración
}
