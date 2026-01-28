// src/types/reportes.ts

// === RESUMEN MENSUAL ===
export interface CencosudResumenMensualItem {
  estado: string;   // 'VENTA', 'COORDINADO', etc.
  cantidad: number;
}

// La API devuelve simplemente un arreglo
export type CencosudResumenMensualResponse = CencosudResumenMensualItem[];

// === LISTADO DE VENTAS ===
export interface CencosudVentaListadoItem {
  idCliente: number;
  dniCliente: string;
  nombreCliente: string;
  telefono: string | null;
  estado: string | null;
  comentario: string | null;
  usuarioCencosud: string | null;
  cargo: string | null;
  supervisor: string | null;
  uunn: string | null;
  nomVendedor: string | null;
  dateEntry: string;        // viene como datetime, lo manejaremos como string
}

export interface CencosudVentasListadoResponse {
  registros: CencosudVentaListadoItem[];
  totalRegistros: number;
  totalFilas: number;
}
