import { useEffect, useState } from "react";
import dayjs from "dayjs";
import Swal from "sweetalert2";
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  Legend,
  CartesianGrid,
  PieChart,  // <-- nuevo
  Pie,       // <-- nuevo
  Cell       // <-- nuevo
} from "recharts";

import {
  obtenerAdminKpis,
  obtenerSerieDiaria,
  obtenerRankingSupervisores,
  obtenerTramitesMes,
  obtenerHoraWapeoPorDia,
  type AdminKpisResponse,
  type SerieDiariaItem,
  type SupervisorRankingItem,
  type TramitesMesRow,
  type HoraWapeoRow
} from "../../services/cencosud/adminReportes.service";

import "./AdminReportesPage.css";

const COLORS = {
  venta: "#22c55e",
  coordinado: "#f59e0b",
  sinOferta: "#ef4444",
  grid: "rgba(255,255,255,0.10)",
  text: "rgba(255,255,255,0.92)",
  muted: "rgba(255,255,255,0.70)",
  card: "rgba(255,255,255,0.08)",
};

const COLOR_PALETTE = ["#4ade80", "#60a5fa", "#f87171"]; // colores para Preevaluados, Regular y Otros
const HORA_COLUMNS = ["08","09","10","11","12","13","14","15","16","17","18","19","20","21","22"];



function pctBadgeClass(pct: number) {
  if (pct >= 100) return "badge badge-ok";
  if (pct >= 70) return "badge badge-warn";
  return "badge badge-bad";
}

function AdminReportesPage() {
  const [periodo, setPeriodo] = useState<string>(dayjs().format("YYYY-MM"));
  const [supervisor] = useState<string>("");
  const [cargando, setCargando] = useState(false);

  const [kpis, setKpis] = useState<AdminKpisResponse | null>(null);
  const [serie, setSerie] = useState<SerieDiariaItem[]>([]);
  const [ranking, setRanking] = useState<SupervisorRankingItem[]>([]);

  const [tramites, setTramites] = useState<TramitesMesRow[]>([]);
  const [horaWapeo, setHoraWapeo] = useState<HoraWapeoRow[]>([]);
  const [filtroTipo, setFiltroTipo] = useState<'TODO' | 'VENTAS'>('TODO');
  const [supervisorFiltro, setSupervisorFiltro] = useState<string>(''); // vacío = todos
  const [asesorFiltro, setAsesorFiltro] = useState<string | null>(null);

  const cargarTodo = async () => {
  try {
    setCargando(true);

    // determina rango de fechas
    const startDate = dayjs(periodo).startOf("month").format("YYYY-MM-DD");
    const endDate = dayjs(periodo).endOf("month").format("YYYY-MM-DD");

    const results = await Promise.allSettled([
      obtenerAdminKpis(periodo),
      obtenerSerieDiaria(periodo, supervisor || undefined),
      obtenerRankingSupervisores(periodo),
      obtenerTramitesMes({
        fechaIni: startDate,
        fechaFin: endDate,
        filtroTipo,
        uunn: null
      }),
      obtenerHoraWapeoPorDia({
        fechaIni: startDate,
        fechaFin: endDate,
        supervisor: supervisorFiltro?.trim() ? supervisorFiltro.trim() : null,
        asesor: asesorFiltro?.trim() ? asesorFiltro.trim() : null,
        uunn: null
      })
    ]);

    const k  = results[0].status === "fulfilled" ? results[0].value : null;
    const s  = results[1].status === "fulfilled" ? results[1].value : [];
    const r  = results[2].status === "fulfilled" ? results[2].value : [];
    const t  = results[3].status === "fulfilled" ? results[3].value : [];
    const hw = results[4].status === "fulfilled" ? results[4].value : [];

    setKpis(k);
    setSerie(s || []);
    setRanking(r || []);
    setTramites(t || []);
    setHoraWapeo(hw || []);
    
    // si quieres, muestra warning SOLO si falla el de hora wapeo
    if (results[4].status === "rejected") {
      console.error("hora-wapeo-por-dia falló:", results[4].reason);
    }

  } catch (err: any) {
    console.error(err);
    const msg =
      err?.response?.data?.mensaje ||
      err?.response?.data?.Mensaje ||
      "No se pudo cargar el reporte admin (revisa endpoints backend).";

    await Swal.fire({
      icon: "error",
      title: "Error",
      text: msg,
      confirmButtonColor: "#d33"
    });

    setKpis(null);
    setSerie([]);
    setRanking([]);
    setTramites([]);
    setHoraWapeo([]);
  } finally {
    setCargando(false);
  }
};


  useEffect(() => {
    cargarTodo();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [periodo, supervisor, filtroTipo, supervisorFiltro, asesorFiltro]);

  return (
    <div className="admrep-page">
      <div className="admrep-shell">
        {/* HEADER */}
        <div className="admrep-header">
          <div>
            <h3 className="admrep-title">Reporte General Admin</h3>
            <p className="admrep-subtitle">
              Indicadores por mes: metas, estados y rendimiento por supervisor/equipo.
            </p>
          </div>

          <div className="admrep-filters">
            <div className="admrep-field">
              <label>Filtro:</label>
              <select value={filtroTipo} onChange={(e) => setFiltroTipo(e.target.value as 'TODO' | 'VENTAS')}>
                <option value="TODO">Todos</option>
                <option value="VENTAS">Solo ventas</option>
              </select>
            </div>
            <div className="admrep-field">
              <label>Supervisor:</label>
              <input type="text" value={supervisorFiltro} onChange={(e) => setSupervisorFiltro(e.target.value)} placeholder="Todos" />
            </div>
            {/* adicionalmente un select para asesor si lo requieres */}

            <button className="admrep-btn" onClick={cargarTodo} disabled={cargando}>
              {cargando ? "Cargando..." : "Refrescar"}
            </button>
          </div>
        </div>

        {/* KPIs */}
        <div className="admrep-kpis">
          <div className="admrep-kpi kpi-venta">
            <div className="kpi-value">{kpis?.ventas ?? 0}</div>
            <div className="kpi-muted">Ventas</div>
          </div>

          <div className="admrep-kpi kpi-coord">
            <div className="kpi-value">{kpis?.coordinado ?? 0}</div>
            <div className="kpi-muted">Coordinación</div>
          </div>

          <div className="admrep-kpi kpi-sinof">
            <div className="kpi-value">{kpis?.sinOferta ?? 0}</div>
            <div className="kpi-muted">Sin oferta</div>
          </div>

        </div>

        {/* Ranking */}
        <div className="admrep-card">
          <div className="admrep-card-head">
            <h4>Ranking por supervisor</h4>
            <span className="admrep-muted">Ventas vs meta (mes)</span>
          </div>

          <div className="admrep-card-body">
            {ranking.length === 0 ? (
              <div className="admrep-empty">No hay data para mostrar.</div>
            ) : (
              <div className="admrep-table-wrap">
                <table className="admrep-table">
                  <thead>
                    <tr>
                      <th>Supervisor</th>
                      <th>Asesores</th>
                      <th>Ventas</th>
                      <th>Meta</th>
                      <th>% Cumpl.</th>
                    </tr>
                  </thead>
                  <tbody>
                    {ranking.map((r) => {
                      const pct = Number(r.cumplimientoPct ?? 0);
                      return (
                        <tr key={r.supervisor}>
                          <td className="td-strong">{r.supervisor}</td>
                          <td>{r.asesores}</td>
                          <td>{r.ventas}</td>
                          <td>{r.metaVentas}</td>
                          <td>
                            <span className={pctBadgeClass(pct)}>{pct.toFixed(2)}%</span>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>

        {/* CHARTS */}
        <div className="admrep-grid">
          {/* Bars */}
          <div className="admrep-card">
            <div className="admrep-card-head">
              <h4>Estados por día</h4>
              <span className="admrep-muted">Barras apiladas (mes)</span>
            </div>

            <div className="admrep-card-body">
              <div className="admrep-chart">
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={serie} margin={{ top: 10, right: 10, bottom: 0, left: 0 }}>
                    <CartesianGrid stroke={COLORS.grid} strokeDasharray="3 3" />
                    <XAxis
                      dataKey="fecha"
                      tick={{ fontSize: 11, fill: COLORS.muted }}
                      tickFormatter={(v) => dayjs(v).format("DD")}
                    />
                    <YAxis tick={{ fontSize: 11, fill: COLORS.muted }} />
                    <Tooltip
                      contentStyle={{
                        background: "rgba(15,23,42,0.96)",
                        border: "1px solid rgba(255,255,255,0.12)",
                        borderRadius: 12,
                        color: COLORS.text,
                      }}
                      labelStyle={{ color: COLORS.text }}
                    />
                    <Legend
                      formatter={(value) => {
                        if (value === "venta") return "VENTA";
                        if (value === "coordinado") return "COORDINADO";
                        if (value === "sinOferta") return "SIN OFERTA";
                        return value;
                      }}
                    />
                    <Bar dataKey="venta" stackId="a" fill={COLORS.venta} radius={[8, 8, 0, 0]} />
                    <Bar dataKey="coordinado" stackId="a" fill={COLORS.coordinado} />
                    <Bar dataKey="sinOferta" stackId="a" fill={COLORS.sinOferta} />
                  </BarChart>
                </ResponsiveContainer>

                <div className="admrep-help">
                  Consejo: si ves picos, filtra por supervisor para analizar su equipo.
                </div>
              </div>
            </div>
          </div>

          {/* Tarjeta: Wapeos por hora */}
          <div className="admrep-card admrep-card-full">
            <div className="admrep-card-head">
              <h4>Wapeos por hora</h4>
              <span className="admrep-muted">Periodo: {periodo}</span>
            </div>
            <div className="admrep-card-body">
              {horaWapeo.length === 0 ? (
                <div className="admrep-empty">No hay data para mostrar.</div>
              ) : (
                <div className="admrep-table-wrap">
                  <table className="admrep-table">
                    <thead>
                      <tr>
                        <th>Supervisor</th>
                        <th>Promotor</th>
                        <th>Total</th>
                        {HORA_COLUMNS.map((h) => (
                          <th key={h}>{h}:00</th>
                        ))}
                      </tr>
                    </thead>
                    <tbody>
                      {horaWapeo.map((row) => (
                        <tr key={`${row.supervisor}-${row.promotor}`}>
                          <td className="td-strong">{row.supervisor}</td>
                          <td>{row.promotor}</td>
                          <td>{row.nroTc}</td>
                          {HORA_COLUMNS.map((h) => (
                            <td key={h}>{row.horas[h] ?? 0}</td>
                          ))}
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </div>

        </div>

      </div>
    </div>
  );
}

export default AdminReportesPage;
