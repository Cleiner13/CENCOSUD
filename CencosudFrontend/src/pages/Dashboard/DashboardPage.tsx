// src/pages/Dashboard/DashboardPage.tsx
import { useMemo } from "react";
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip } from "recharts";
import { Link } from "react-router-dom";
import "./DashboardPage.css";

import { useDashboardPage } from "../../hooks/useDashboardPage";
import type { DashboardChartPoint } from "../../hooks/useDashboardPage";

const COLORS = ["#FF6B6B", "#FFD93D", "#6BCB77", "#4D96FF", "#E56B6F"];

export default function DashboardPage() {
  const {
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
  } = useDashboardPage();

  // ================= RENDER =================
  return (
    <div className="dashboard-page">
      <section className="dashboard-hero">
        <p className="dashboard-hello">
          Bienvenido, <span>{usuario || "Usuario"}</span>
        </p>

        <h2 className="dashboard-title">{chartTitle}</h2>
        <p className="dashboard-subtitle">{chartSubtitle}</p>

        {/* CONTROLES (Mes + métrica) */}
        <div style={{ display: "flex", gap: 10, alignItems: "end", marginBottom: 10 }}>
          <div style={{ minWidth: 160 }}>
            <label className="form-label" style={{ color: "rgba(255,255,255,0.85)", fontSize: 12 }}>
              Mes
            </label>
            <input
              type="month"
              className="form-control form-control-sm"
              value={mesSeleccionado}
              onChange={(e) => setMesSeleccionado(e.target.value)}
            />
          </div>

          {(esSupervisor || esAdmin) && (
            <div style={{ display: "flex", gap: 8 }}>
              <button
                type="button"
                className={`btn btn-sm ${metrica === "WAPEOS" ? "btn-primary" : "btn-outline-light"}`}
                onClick={() => setMetrica("WAPEOS")}
              >
                Wapeos
              </button>
              <button
                type="button"
                className={`btn btn-sm ${metrica === "VENTAS" ? "btn-primary" : "btn-outline-light"}`}
                onClick={() => setMetrica("VENTAS")}
              >
                Ventas
              </button>
            </div>
          )}
        </div>

        <div className="dashboard-chart-box">
          {loading && <div style={{ padding: 10, color: "rgba(255,255,255,0.85)", fontSize: 12 }}>Cargando...</div>}
          {error && <div style={{ padding: 10, color: "#FFB4B4", fontSize: 12 }}>{error}</div>}

          {!loading && !error && (
            <>
              <div className="dashboard-chart-wrap">
                <ResponsiveContainer width="100%" height={260}>
                  <PieChart>
                    <Pie
                      data={chartData}
                      dataKey="value"
                      nameKey="name"
                      innerRadius={78}
                      outerRadius={110}
                      paddingAngle={3}
                      cornerRadius={8}
                      stroke="transparent"
                    >
                      {chartData.map((entry, index) => (
                        <Cell key={entry.name} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>

                    <Tooltip
                      contentStyle={{
                        backgroundColor: "#0b1220",
                        border: "1px solid rgba(255,255,255,0.12)",
                        borderRadius: 12,
                        color: "#e5e7eb",
                        fontSize: 12,
                      }}
                      itemStyle={{ color: "#e5e7eb" }}
                      cursor={{ fill: "rgba(255,255,255,0.06)" }}
                    />
                  </PieChart>
                </ResponsiveContainer>

                <div className="dashboard-chart-center">
                  <div className="dashboard-total-label">TOTAL</div>
                  <div className="dashboard-total-value">{totalCentro}</div>
                </div>
              </div>

              <div className="dashboard-kpi-list">
                {chartData.map((item: DashboardChartPoint, idx: number) => (
                  <div key={item.name} className="dashboard-kpi-row">
                    <div className="dashboard-kpi-left">
                      <span className="dashboard-kpi-dot" style={{ backgroundColor: COLORS[idx % COLORS.length] }} />
                      <span className="dashboard-kpi-name">{item.name}</span>
                    </div>
                    <div className="dashboard-kpi-value">{item.value}</div>
                  </div>
                ))}

                {/* ================= METAS SIEMPRE ================= */}
                <div className="dashboard-metas-title">Metas del mes ({mesLabel})</div>

                {/* ASESOR: metas individuales */}
                {esAsesor && (
                  <>
                    <div className="dashboard-meta-row">
                      <span>Meta Wapeos</span>
                      <b>{metaWapeos}</b>
                    </div>
                    <div className="dashboard-meta-row">
                      <span>Meta Ventas</span>
                      <b>{metaVentas}</b>
                    </div>

                    {(metaWapeos > 0 || metaVentas > 0) ? (
                      <>
                        {metaWapeos > 0 && !logroWapeos && (
                          <div className="dashboard-meta-hint">
                            Te faltan <b>{faltanWapeos}</b> wapeos para llegar a la meta.
                          </div>
                        )}
                        {metaVentas > 0 && !logroVentas && (
                          <div className="dashboard-meta-hint">
                            Te faltan <b>{faltanVentas}</b> ventas para llegar a la meta.
                          </div>
                        )}

                        {(logroWapeos || logroVentas) && (
                          <div className="dashboard-meta-success">
                            <div><b>¡Lo lograste!</b> llegaste a la META.</div>
                            <div style={{ fontSize: 12, opacity: 0.85, marginTop: 4 }}>Obtener recompensa</div>
                            <Link to="/recompensa" className="dashboard-meta-btn">
                              Obtener recompensa
                            </Link>
                          </div>
                        )}
                      </>
                    ) : (
                      <div className="dashboard-meta-hint">Aún no tienes metas asignadas por el momento.</div>
                    )}
                  </>
                )}

                {/* SUPERVISOR / ADMIN: metas equipo + faltantes */}
                {(esSupervisor || esAdmin) && (
                  <>
                    <div className="dashboard-meta-row">
                      <span>Meta Equipo Wapeos</span>
                      <b>{metaEquipoWapeos}</b>
                    </div>
                    <div className="dashboard-meta-row">
                      <span>Meta Equipo Ventas</span>
                      <b>{metaEquipoVentas}</b>
                    </div>

                    {(metaEquipoWapeos > 0 || metaEquipoVentas > 0) ? (
                      <>
                        {metaEquipoWapeos > 0 && !logroEquipoWapeos && (
                          <div className="dashboard-meta-hint">
                            Te faltan <b>{faltanEquipoWapeos}</b> wapeos para llegar a la meta del equipo.
                          </div>
                        )}
                        {metaEquipoVentas > 0 && !logroEquipoVentas && (
                          <div className="dashboard-meta-hint">
                            Te faltan <b>{faltanEquipoVentas}</b> ventas para llegar a la meta del equipo.
                          </div>
                        )}

                        {(logroEquipoWapeos || logroEquipoVentas) && (
                          <div className="dashboard-meta-success">
                            <div><b>¡Meta lograda!</b> el equipo alcanzó la META.</div>
                          </div>
                        )}
                      </>
                    ) : (
                      <div className="dashboard-meta-hint">Aún no hay metas de equipo asignadas por el momento.</div>
                    )}
                  </>
                )}
              </div>
            </>
          )}
        </div>
      </section>

      <section className="dashboard-quick">
        <h3 className="quick-title">MENU PRINCIPAL</h3>

        <div className="quick-grid">
          <div className="quick-card">
            <div className="quick-icon">📝</div>
            <div className="quick-body">
              <div className="quick-card-title">Formulario de ventas</div>
              <div className="quick-card-text">Sube la captura del wapeo y registra la venta .</div>
              <Link to="/registro" className="quick-btn">Ir al registro</Link>
            </div>
          </div>

          <div className="quick-card">
            <div className="quick-icon">📊</div>
            <div className="quick-body">
              <div className="quick-card-title">Reportes y seguimiento</div>
              <div className="quick-card-text">Reportes de ventas, por fechas y estado de las ventas registradas.</div>
              <Link to="/reportes" className="quick-btn">Ir a reportes</Link>
            </div>
          </div>

          {(esSupervisor || esAdmin) && (
            <div className="quick-card">
              <div className="quick-icon">🎯</div>
              <div className="quick-body">
                <div className="quick-card-title">Panel de metas</div>
                <div className="quick-card-text">
                  Configura metas mensuales (wapeos y ventas) para tu equipo.
                </div>
                <Link to="/metas" className="quick-btn">
                  Ir a metas
                </Link>
              </div>
            </div>
          )}

          {esAdmin && (
            <div className="quick-card">
              <div className="quick-icon">💼</div>
              <div className="quick-body">
                <div className="quick-card-title">Reporte General Admin</div>
                <div className="quick-card-text">
                  Visualiza de forma detallada las ventas y el avance por metas.
                </div>
                <Link to="/admin-reportes" className="quick-btn">
                  Ir a gráficos
                </Link>
              </div>
            </div>
          )}


        </div>
      </section>
    </div>
  );
}
