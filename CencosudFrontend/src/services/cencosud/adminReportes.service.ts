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

export interface TramitesMesRequest {
  fechaIni: string;    // ISO string 'YYYY-MM-DD'
  fechaFin: string;
  filtroTipo: 'TODO' | 'VENTAS';
  uunn?: string | null;
}

export interface TramitesMesRow {
  tipoTramite: string;  // 'Preevaluados', 'Regular' u 'Otros'
  cantidad: number;
  porcentaje: number;
}

export interface HoraWapeoRequest {
  fechaIni: string;
  fechaFin: string;
  supervisor?: string | null; // <-- permitir null
  asesor?: string | null;
  uunn?: string | null;
}

export interface HoraWapeoRow {
  supervisor: string;
  promotor: string;
  nroTc: number;
  horas: Record<string, number>; // Ej: { "08": 0, "09": 2, ... }
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

export async function obtenerTramitesMes(req: TramitesMesRequest): Promise<TramitesMesRow[]> {
  const resp = await apiClient.post('/CencosudAdminReportes/tramites-mes', req);
  return resp.data;
}

export async function obtenerHoraWapeoPorDia(req: HoraWapeoRequest): Promise<HoraWapeoRow[]> {
  console.log("REQ hora-wapeo-por-dia =>", req);
  const resp = await apiClient.post('/CencosudAdminReportes/hora-wapeo-por-dia', req);
  return resp.data;
}