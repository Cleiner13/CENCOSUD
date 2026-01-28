export type DashboardTotalesDto = {
  totalWapeos: number;
  totalVentas: number;

  metaWapeos?: number | null;
  metaVentas?: number | null;

  metaWapeosEquipo?: number | null;
  metaVentasEquipo?: number | null;

  avanceWapeosPct?: number | null;
  avanceVentasPct?: number | null;
};

export type DashboardAsesorEstadoDto = {
  estado: string;
  cantidad: number;
};

export type DashboardAsesorResponse = {
  estados: DashboardAsesorEstadoDto[];
  totales: DashboardTotalesDto;
};

// Para hacerlo tolerante a tu SP (por si aún no trae wapeos/ventas separados)
export type DashboardSupervisorDetalleItem = {
  asesor: string;
  wapeos?: number;
  ventas?: number;
  cantidad?: number; // fallback
};

export type DashboardSupervisorResponse = {
  detalle: DashboardSupervisorDetalleItem[];
  totales: DashboardTotalesDto;
};

export type DashboardAdminDetalleItem = {
  supervisor: string;
  wapeos?: number;
  ventas?: number;
  cantidad?: number; // fallback
};

export type DashboardAdminResponse = {
  detalle: DashboardAdminDetalleItem[];
  totales: DashboardTotalesDto;
};
