import { useEffect, useMemo, useState } from 'react';
import type {
  DashboardAdminResponse,
  DashboardAsesorResponse,
  DashboardSupervisorResponse,
  DashboardTotalesDto,
} from '../types/dashboard';
import {
  obtenerDashboardAdmin,
  obtenerDashboardAsesor,
  obtenerDashboardSupervisor,
} from '../services/dashboard/dashboard.service';

export type MetricaDashboard = 'WAPEOS' | 'VENTAS';

export type DashboardChartPoint = { name: string; value: number };

const ESTADOS_FIJOS = ['VENTA', 'COORDINADO', 'SIN OFERTA'] as const;

function normalizarEstado(raw: string) {
  const s = (raw || '').trim().toUpperCase();
  if (s.startsWith('VENT')) return 'VENTA';
  if (s.startsWith('COORD')) return 'COORDINADO';
  if (s.includes('SIN')) return 'SIN OFERTA';
  return s;
}

function getMesDefaultYYYYMMdash() {
  const now = new Date();
  const y = now.getFullYear();
  const m = String(now.getMonth() + 1).padStart(2, '0');
  return `${y}-${m}`;
}

function formatearMes(yyyyDashMm: string) {
  const d = new Date(`${yyyyDashMm}-01T00:00:00`);
  const mes = d.toLocaleString('es-PE', { month: 'long' });
  const anio = d.getFullYear();
  return `${mes.charAt(0).toUpperCase()}${mes.slice(1)} ${anio}`;
}

function rangoDelMes(yyyyDashMm: string) {
  const [yy, mm] = yyyyDashMm.split('-');
  const y = Number(yy);
  const m = Number(mm);

  const fechaIni = `${y}-${String(m).padStart(2, '0')}-01`;
  const lastDay = new Date(y, m, 0).getDate();
  const fechaFin = `${y}-${String(m).padStart(2, '0')}-${String(lastDay).padStart(2, '0')}`;

  return { fechaIni, fechaFin };
}

export interface UseDashboardPageResult {
  usuario: string;
  esAsesor: boolean;
  esSupervisor: boolean;
  esAdmin: boolean;

  loading: boolean;
  error: string | null;

  mesSeleccionado: string;
  setMesSeleccionado: (value: string) => void;
  mesLabel: string;

  metrica: MetricaDashboard;
  setMetrica: (value: MetricaDashboard) => void;

  chartTitle: string;
  chartSubtitle: string;
  chartData: DashboardChartPoint[];
  totalCentro: number;

  totales: DashboardTotalesDto | null;
  // metas individuales
  metaWapeos: number;
  metaVentas: number;
  faltanWapeos: number;
  faltanVentas: number;
  logroWapeos: boolean;
  logroVentas: boolean;
  // metas equipo
  metaEquipoWapeos: number;
  metaEquipoVentas: number;
  faltanEquipoWapeos: number;
  faltanEquipoVentas: number;
  logroEquipoWapeos: boolean;
  logroEquipoVentas: boolean;
}

export function useDashboardPage(): UseDashboardPageResult {
  const cargoRaw = localStorage.getItem('cargo') || '';
  const cargo = cargoRaw.toUpperCase();
  const usuario = localStorage.getItem('usuario') || '';
  const uunnSesion = localStorage.getItem('uunn') || '';

  const rolAppRaw = (localStorage.getItem('rolApp') || '').toUpperCase();

  let esAsesor = rolAppRaw === 'ASESOR';
  let esSupervisor = rolAppRaw === 'SUPERVISOR';
  let esAdmin = rolAppRaw === 'ADMIN';

  if (!rolAppRaw) {
    esAsesor = cargo.includes('PROMOTOR');
    esSupervisor = cargo.includes('SUPERVISOR');
    esAdmin = !esAsesor && !esSupervisor;
  }

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [metrica, setMetrica] = useState<MetricaDashboard>('WAPEOS');

  const [dataAsesor, setDataAsesor] = useState<DashboardAsesorResponse | null>(null);
  const [dataSupervisor, setDataSupervisor] = useState<DashboardSupervisorResponse | null>(null);
  const [dataAdmin, setDataAdmin] = useState<DashboardAdminResponse | null>(null);

  const [mesSeleccionado, setMesSeleccionado] = useState<string>(getMesDefaultYYYYMMdash);

  const uunnDashboard = useMemo(() => {
    if (esAdmin) return 'CENCOSUD TC';
    return uunnSesion;
  }, [esAdmin, uunnSesion]);

  const mesLabel = useMemo(() => formatearMes(mesSeleccionado), [mesSeleccionado]);

  const { fechaIni, fechaFin } = useMemo(
    () => rangoDelMes(mesSeleccionado),
    [mesSeleccionado]
  );

  useEffect(() => {
    const cargar = async () => {
      try {
        setLoading(true);
        setError(null);

        if (!uunnDashboard) throw new Error('No se encontró UUNN/campaña para dashboard.');

        if (esAdmin) {
          const resp = await obtenerDashboardAdmin({ uunn: uunnDashboard, fechaIni, fechaFin });
          setDataAdmin(resp);
          setDataSupervisor(null);
          setDataAsesor(null);
        } else if (esSupervisor) {
          const resp = await obtenerDashboardSupervisor({
            uunn: uunnDashboard,
            supervisor: usuario,
            fechaIni,
            fechaFin,
          });
          setDataSupervisor(resp);
          setDataAdmin(null);
          setDataAsesor(null);
        } else {
          const resp = await obtenerDashboardAsesor({
            uunn: uunnDashboard,
            asesor: usuario,
            fechaIni,
            fechaFin,
          });
          setDataAsesor(resp);
          setDataAdmin(null);
          setDataSupervisor(null);
        }
      } catch (e: any) {
        setError(e?.response?.data?.message || e?.message || 'Error cargando dashboard.');
      } finally {
        setLoading(false);
      }
    };

    void cargar();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [uunnDashboard, mesSeleccionado]);

  const chartTitle = useMemo(() => {
    if (esAdmin) return 'Ventas mensuales por supervisor';
    if (esSupervisor) return 'Ventas de tu equipo';
    return 'Tus registros del mes';
  }, [esAdmin, esSupervisor]);

  const chartSubtitle = useMemo(() => {
    if (esAdmin)
      return metrica === 'WAPEOS'
        ? 'Total de wapeos por supervisor'
        : 'Total de ventas por supervisor';
    if (esSupervisor)
      return metrica === 'WAPEOS'
        ? 'Total de wapeos por asesor'
        : 'Total de ventas por asesor';
    return 'Distribución por estado de gestión';
  }, [esAdmin, esSupervisor, metrica]);

  const chartData: DashboardChartPoint[] = useMemo(() => {
    if (esAdmin && dataAdmin) {
      return dataAdmin.detalle.map((x) => ({
        name: x.supervisor,
        value:
          metrica === 'VENTAS'
            ? x.ventas ?? x.cantidad ?? 0
            : x.wapeos ?? x.cantidad ?? 0,
      }));
    }

    if (esSupervisor && dataSupervisor) {
      return dataSupervisor.detalle.map((x) => ({
        name: x.asesor,
        value:
          metrica === 'VENTAS'
            ? x.ventas ?? x.cantidad ?? 0
            : x.wapeos ?? x.cantidad ?? 0,
      }));
    }

    if (esAsesor && dataAsesor) {
      const map = new Map<string, number>();
      for (const e of dataAsesor.estados) {
        map.set(normalizarEstado(e.estado), e.cantidad ?? 0);
      }

      return ESTADOS_FIJOS.map((st) => ({
        name: st,
        value: map.get(st) ?? 0,
      }));
    }

    return [];
  }, [esAdmin, esSupervisor, esAsesor, dataAdmin, dataSupervisor, dataAsesor, metrica]);

  const totales: DashboardTotalesDto | null = useMemo(() => {
    if (esAdmin) return dataAdmin?.totales ?? null;
    if (esSupervisor) return dataSupervisor?.totales ?? null;
    return dataAsesor?.totales ?? null;
  }, [esAdmin, esSupervisor, dataAdmin, dataSupervisor, dataAsesor]);

  const totalCentro = useMemo(() => {
    if (esAsesor) return chartData.reduce((acc, it) => acc + it.value, 0);
    if (totales)
      return metrica === 'VENTAS'
        ? totales.totalVentas ?? 0
        : totales.totalWapeos ?? 0;

    return chartData.reduce((acc, it) => acc + it.value, 0);
  }, [esAsesor, chartData, totales, metrica]);

  const actualWapeos = useMemo(() => {
    if (esAsesor) return totalCentro;
    return totales?.totalWapeos ?? 0;
  }, [esAsesor, totalCentro, totales]);

  const actualVentas = useMemo(() => {
    if (esAsesor) {
      const venta = chartData.find((x) => x.name === 'VENTA')?.value ?? 0;
      return venta;
    }
    return totales?.totalVentas ?? 0;
  }, [esAsesor, chartData, totales]);

  const metaWapeos = useMemo(() => totales?.metaWapeos ?? 0, [totales]);
  const metaVentas = useMemo(() => totales?.metaVentas ?? 0, [totales]);

  const faltanWapeos = useMemo(
    () => (metaWapeos > 0 ? Math.max(0, metaWapeos - actualWapeos) : 0),
    [metaWapeos, actualWapeos]
  );
  const faltanVentas = useMemo(
    () => (metaVentas > 0 ? Math.max(0, metaVentas - actualVentas) : 0),
    [metaVentas, actualVentas]
  );

  const logroWapeos = metaWapeos > 0 && actualWapeos >= metaWapeos;
  const logroVentas = metaVentas > 0 && actualVentas >= metaVentas;

  const metaEquipoWapeos = useMemo(
    () => totales?.metaWapeosEquipo ?? 0,
    [totales]
  );
  const metaEquipoVentas = useMemo(
    () => totales?.metaVentasEquipo ?? 0,
    [totales]
  );

  const faltanEquipoWapeos = useMemo(
    () =>
      metaEquipoWapeos > 0
        ? Math.max(0, metaEquipoWapeos - actualWapeos)
        : 0,
    [metaEquipoWapeos, actualWapeos]
  );
  const faltanEquipoVentas = useMemo(
    () =>
      metaEquipoVentas > 0
        ? Math.max(0, metaEquipoVentas - actualVentas)
        : 0,
    [metaEquipoVentas, actualVentas]
  );

  const logroEquipoWapeos =
    metaEquipoWapeos > 0 && actualWapeos >= metaEquipoWapeos;
  const logroEquipoVentas =
    metaEquipoVentas > 0 && actualVentas >= metaEquipoVentas;

  return {
    usuario,
    esAsesor,
    esSupervisor,
    esAdmin,
    loading,
    error,
    mesSeleccionado,
    setMesSeleccionado,
    mesLabel,
    metrica,
    setMetrica,
    chartTitle,
    chartSubtitle,
    chartData,
    totalCentro,
    totales,
    metaWapeos,
    metaVentas,
    faltanWapeos,
    faltanVentas,
    logroWapeos,
    logroVentas,
    metaEquipoWapeos,
    metaEquipoVentas,
    faltanEquipoWapeos,
    faltanEquipoVentas,
    logroEquipoWapeos,
    logroEquipoVentas,
  };
}

