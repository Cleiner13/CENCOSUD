import apiClient from "../http/apiClient";
import type {
  DashboardAsesorResponse,
  DashboardSupervisorResponse,
  DashboardAdminResponse,
} from "../../types/dashboard";

export async function obtenerDashboardAsesor(params: {
  uunn: string;
  asesor: string;
  fechaIni?: string;
  fechaFin?: string;
}): Promise<DashboardAsesorResponse> {
  const { data } = await apiClient.get<DashboardAsesorResponse>(
    "/Dashboard/asesor",
    { params }
  );
  return data;
}

export async function obtenerDashboardSupervisor(params: {
  uunn: string;
  supervisor: string;
  fechaIni?: string;
  fechaFin?: string;
}): Promise<DashboardSupervisorResponse> {
  const { data } = await apiClient.get<DashboardSupervisorResponse>(
    "/Dashboard/supervisor",
    { params }
  );
  return data;
}

export async function obtenerDashboardAdmin(params: {
  uunn: string;
  fechaIni?: string;
  fechaFin?: string;
}): Promise<DashboardAdminResponse> {
  const { data } = await apiClient.get<DashboardAdminResponse>(
    "/Dashboard/admin",
    { params }
  );
  return data;
}
