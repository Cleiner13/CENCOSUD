// src/services/cencosud/reportes.service.ts
import apiClient from '../http/apiClient';
import type {
  CencosudResumenMensualResponse,
  CencosudVentasListadoResponse,
} from '../../types/reportes';

export interface ListadoVentasParams {
  fechaIni?: string;
  fechaFin?: string;
  estado?: string;
  dniCliente?: string;
  vendedor?: string; // ✅ filtrar por asesor
  page?: number;
  pageSize?: number;
}

// === RESUMEN MENSUAL ===
export async function obtenerResumenMensual(
  fechaIni?: string,
  fechaFin?: string
): Promise<CencosudResumenMensualResponse> {
  const { data } = await apiClient.get<CencosudResumenMensualResponse>(
    '/CencosudReportes/resumen-mensual',
    { params: { fechaIni, fechaFin } }
  );
  return data;
}

// === LISTADO DE VENTAS (paginado) ===
export async function obtenerListadoVentas(
  params: ListadoVentasParams
): Promise<CencosudVentasListadoResponse> {
  const { data } = await apiClient.get<CencosudVentasListadoResponse>(
    '/CencosudReportes/listado',
    { params }
  );
  return data;
}

// ✅ NUEVO: LISTAR ASESORES DEL SUPERVISOR LOGUEADO
export async function obtenerAsesoresSupervisor(uunn?: string): Promise<string[]> {
  const { data } = await apiClient.get<string[]>(
    '/CencosudReportes/asesores',
    { params: { uunn } }
  );
  return data;
}

// === DESCARGAR EXCEL ===
export async function descargarListadoVentasExcel(
  params: ListadoVentasParams
): Promise<void> {
  // ✅ Mandar solo filtros (sin page/pageSize)
  const filtros = {
    fechaIni: params.fechaIni,
    fechaFin: params.fechaFin,
    estado: params.estado,
    dniCliente: params.dniCliente,
    vendedor: params.vendedor,
  };

  const response = await apiClient.get('/CencosudReportes/listado-excel', {
    params: filtros,
    responseType: 'blob',
  });

  const blob = new Blob([response.data], {
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  });

  const url = window.URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;

  const ini = (params.fechaIni ?? 'inicio').replaceAll('-', '');
  const fin = (params.fechaFin ?? 'fin').replaceAll('-', '');

  link.download = `ventas_cencosud_${ini}_${fin}.xlsx`;
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
}

// === ELIMINAR VENTA (solo ADMIN) ===
export async function eliminarVenta(idCliente: number): Promise<void> {
  await apiClient.delete(`/CencosudTienda/cliente/${idCliente}`);
}
