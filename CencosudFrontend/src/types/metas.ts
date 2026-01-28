// src/types/metas.ts

export interface MetaAsesorDto {
  uunn: string;
  periodo: number;          // YYYYMM (ej: 202512)
  supervisor: string;
  asesor: string;
  metaWapeos?: number | null;
  metaVentas?: number | null;
  usuarioAccion?: string | null;
}

export interface MetaSupervisorDto {
  uunn: string;
  periodo: number;          // YYYYMM
  supervisor: string;
  metaWapeos?: number | null;
  metaVentas?: number | null;
  usuarioAccion?: string | null;
}
