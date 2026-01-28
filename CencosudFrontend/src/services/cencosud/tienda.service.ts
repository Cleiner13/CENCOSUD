// src/services/cencosudTiendaService.ts
import apiClient from '../http/apiClient';
import type {
  CencosudClienteRequest,
  CencosudClienteResponse,
  CencosudClienteDetalle,
} from '../../types/cencosud';

// Guardar / actualizar
export async function guardarClienteCencosud(
  payload: CencosudClienteRequest
): Promise<CencosudClienteResponse> {
  const { data } = await apiClient.post<CencosudClienteResponse>(
    '/CencosudTienda/guardar', // 👈 OBLIGATORIO que sea así
    payload
  );
  return data;
}

// Obtener detalle para editar
export async function obtenerClientePorId(
  idCliente: number
): Promise<CencosudClienteDetalle> {
  const { data } = await apiClient.get<CencosudClienteDetalle>(
    `/CencosudTienda/cliente/${idCliente}` // 👈 mismo prefijo
  );
  return data;
}
