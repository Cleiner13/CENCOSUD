import apiClient from "../http/apiClient";

export type EstadoVenta = "VENTA" | "COORDINADO" | "SIN OFERTA";

export interface AdminKpisResponse {
  periodo: string; // "2026-01"
  ventas: number;
  coordinado: number;
  sinOferta: number;

  metaEquipoVentas: number;      // meta global del mes (o suma de equipos)
  cumplimientoVentasPct: number; // 0..100

  metaEquipoWapeos?: number;
  cumplimientoWapeosPct?: number;
}

export interface SerieDiariaItem {
  fecha: string; // "2026-01-05"
  venta: number;
  coordinado: number;
  sinOferta: number;
}

export interface SupervisorRankingItem {
  supervisor: string;
  ventas: number;
  metaVentas: number;
  cumplimientoPct: number;
  asesores: number;
}

export async function obtenerAdminKpis(periodo: string) {
  const { data } = await apiClient.get<AdminKpisResponse>(
    "/CencosudAdminReportes/kpis",
    { params: { periodo } }
  );
  return data;
}

export async function obtenerSerieDiaria(periodo: string, supervisor?: string) {
  const { data } = await apiClient.get<SerieDiariaItem[]>(
    "/CencosudAdminReportes/serie-diaria",
    { params: { periodo, supervisor } }
  );
  return data;
}

export async function obtenerRankingSupervisores(periodo: string) {
  const { data } = await apiClient.get<SupervisorRankingItem[]>(
    "/CencosudAdminReportes/ranking-supervisores",
    { params: { periodo } }
  );
  return data;
}
