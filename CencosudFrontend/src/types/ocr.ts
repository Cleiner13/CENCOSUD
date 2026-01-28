// src/types/ocr.ts
export interface OcrCencosudCampos {
  fecha: string | null;
  tipo_doc: string | null;
  doc: string | null;
  nombre: string | null;
  tipo_de_tramite: string | null;
  oferta: string | null;
  incremento_de_linea: string | null;
  superavance: string | null;
  superavance_plus: string | null;
  avance_efectivo: string | null;
  cambio_de_producto: string | null;
}

export interface OcrCencosudResponse {
  campos: OcrCencosudCampos;
  rawJson: string; // o TextoCrudo / RawJson según tu DTO
}
