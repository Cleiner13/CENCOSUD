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

  // Campos dinámicos
  infoAdicional?: Record<string, string>;
  tieneImagen: boolean;
}

export interface CencosudClienteResponse {
  idCliente?: number | null;
  codigoResultado: number;
  mensaje: string;
}


// src/types/cencosud.ts
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

  // Campos dinámicos (de CENCOSUD_TIENDA_CLIENTES_INFORMACION)
  avanceEfectivo?: string | null;
  tipoTramite?: string | null;
  oferta?: string | null;
  superavance?: string | null;
  cambioProducto?: string | null;
  incrementoLinea?: string | null;
  // Nuevo: campos dinámicos
  infoAdicional?: Record<string, string>;
}

