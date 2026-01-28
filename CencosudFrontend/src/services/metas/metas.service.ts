// src/services/metas/metas.service.ts
import apiClient from "../http/apiClient";
import type { MetaAsesorDto, MetaSupervisorDto } from "../../types/metas";

export async function obtenerMetasAsesores(params: {
  uunn: string;
  periodo: number;
}): Promise<MetaAsesorDto[]> {
  const { data } = await apiClient.get<MetaAsesorDto[]>(
    "/Metas/asesores",
    { params }
  );
  return data;
}

export async function guardarMetaAsesor(dto: MetaAsesorDto): Promise<void> {
  await apiClient.post("/Metas/asesor", dto);
}

export async function obtenerMetasSupervisores(params: {
  uunn: string;
  periodo: number;
}): Promise<MetaSupervisorDto[]> {
  const { data } = await apiClient.get<MetaSupervisorDto[]>(
    "/Metas/supervisores",
    { params }
  );
  return data;
}

export async function guardarMetaSupervisor(dto: MetaSupervisorDto): Promise<void> {
  await apiClient.post("/Metas/supervisor", dto);
}
