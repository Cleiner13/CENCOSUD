// src/types/cencosud.ts

export interface CencosudClienteRequest {
  idCliente?: number | null;

  dniCliente: string;
  nombreCliente: string;
  telefono: string;
  estado?: string | null;
  comentario?: string | null;
  usuarioCencosud?: string | null;
  cargo?: string | null;
  supervisor?: string | null;
  uunn?: string | null;
  nomVendedor?: string | null;

  /**
   * Campos dinámicos clave/valor. Aquí incluirás las nuevas claves como
   * "Adicionales", "Efectivo Cencosud", "EC Pct", etc. al momento de guardar.
   */
  infoAdicional?: Record<string, string>;
  tieneImagen: boolean;
}

export interface CencosudClienteResponse {
  idCliente?: number | null;
  codigoResultado: number;
  mensaje: string;
}

export interface CencosudClienteDetalle {
  idCliente: number;
  dniCliente: string;
  nombreCliente: string;
  telefono: string;

  estado?: string | null;
  comentario?: string | null;
  usuarioCencosud?: string | null;
  cargo?: string | null;
  supervisor?: string | null;
  uunn?: string | null;
  nomVendedor?: string | null;

  dateEntry?: string | null;
  dateModify?: string | null;

  // Campos fijos existentes en la tabla de información
  avanceEfectivo?: string | null;
  tipoTramite?: string | null;
  oferta?: string | null;
  superavance?: string | null;
  cambioProducto?: string | null;
  incrementoLinea?: string | null;

  // Nuevos campos individuales devueltos desde la BD
  adicionales?: string | null;
  efectivoCencosud?: string | null;
  ecPct?: string | null;
  ecTasa?: string | null;
  aePct?: string | null;
  aeTasa?: string | null;

  /**
   * Diccionario con cualquier campo dinámico adicional. Contendrá los mismos
   * valores de arriba y otros que puedan añadirse en el futuro si decides
   * usar InfoAdicional para más claves.
   */
  infoAdicional?: Record<string, string>;
}
