// src/pages/Metas/MetasPage.tsx
import { useEffect, useMemo, useState } from "react";
import dayjs from "dayjs";
import Swal from "sweetalert2";
import { useNavigate } from "react-router-dom";
import "./MetasPage.css";

import {
  obtenerMetasAsesores,
  guardarMetaAsesor,
  obtenerMetasSupervisores,
  guardarMetaSupervisor,
} from "../../services/metas/metas.service";

import type { MetaAsesorDto, MetaSupervisorDto } from "../../types/metas";

type RolApp = "ASESOR" | "SUPERVISOR" | "ADMIN";

function detectarRol(): RolApp {
  const cargoRaw = localStorage.getItem("cargo") || "";
  const cargo = cargoRaw.toUpperCase();
  const rolAppLocal = (localStorage.getItem("rolApp") || "").trim().toUpperCase();

  if (rolAppLocal === "ASESOR" || rolAppLocal === "SUPERVISOR" || rolAppLocal === "ADMIN") {
    return rolAppLocal as RolApp;
  }

  if (cargo.includes("PROMOTOR")) return "ASESOR";
  if (cargo.includes("SUPERVISOR")) return "SUPERVISOR";
  return "ADMIN";
}

function yyyymmFromMonthInput(valueYYYYMMdash: string): number {
  const [y, m] = valueYYYYMMdash.split("-");
  return Number(`${y}${m}`);
}

function monthInputFromYYYYMM(periodo: number): string {
  const s = String(periodo);
  return `${s.slice(0, 4)}-${s.slice(4, 6)}`;
}

export default function MetasPage() {
  const navigate = useNavigate();
  const rolApp = useMemo(() => detectarRol(), []);

  const usuario = localStorage.getItem("usuario") || "";
  const uunnSesion = localStorage.getItem("uunn") || "";

  const [uunn, setUunn] = useState<string>(rolApp === "ADMIN" ? "CENCOSUD TC" : uunnSesion);

  const periodoDefault = Number(dayjs().format("YYYYMM"));
  const [periodo, setPeriodo] = useState<number>(periodoDefault);

  const [loading, setLoading] = useState<boolean>(true);
  const [search, setSearch] = useState<string>("");

  const [metasAsesores, setMetasAsesores] = useState<MetaAsesorDto[]>([]);
  const [metasSupervisores, setMetasSupervisores] = useState<MetaSupervisorDto[]>([]);

  const [aplicarWapeos, setAplicarWapeos] = useState<number>(0);
  const [aplicarVentas, setAplicarVentas] = useState<number>(0);

  useEffect(() => {
    if (rolApp === "ASESOR") {
      Swal.fire("Sin acceso", "Este módulo es solo para SUPERVISOR y ADMIN.", "warning");
      navigate("/dashboard", { replace: true });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [rolApp]);

  const cargar = async () => {
    try {
      setLoading(true);

      if (!uunn) {
        await Swal.fire("Falta campaña", "No se encontró UUNN/Campaña.", "error");
        return;
      }

      if (rolApp === "SUPERVISOR") {
        const data = await obtenerMetasAsesores({ uunn, periodo });
        setMetasAsesores(
          (data || []).map((x) => ({
            ...x,
            metaWapeos: x.metaWapeos ?? 0,
            metaVentas: x.metaVentas ?? 0,
          }))
        );
      } else if (rolApp === "ADMIN") {
        const data = await obtenerMetasSupervisores({ uunn, periodo });
        setMetasSupervisores(
          (data || []).map((x) => ({
            ...x,
            metaWapeos: x.metaWapeos ?? 0,
            metaVentas: x.metaVentas ?? 0,
          }))
        );
      }
    } catch (e: any) {
      const msg =
        e?.response?.data?.message ||
        e?.response?.data?.title ||
        JSON.stringify(e?.response?.data) ||
        e?.message ||
        "No se pudo cargar metas.";
      await Swal.fire("Error", String(msg), "error");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    cargar();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [uunn, periodo, rolApp]);

  const asesoresFiltrados = useMemo(() => {
    const q = search.trim().toUpperCase();
    if (!q) return metasAsesores;
    return metasAsesores.filter((x) => (x.asesor || "").toUpperCase().includes(q));
  }, [metasAsesores, search]);

  const supervisoresFiltrados = useMemo(() => {
    const q = search.trim().toUpperCase();
    if (!q) return metasSupervisores;
    return metasSupervisores.filter((x) => (x.supervisor || "").toUpperCase().includes(q));
  }, [metasSupervisores, search]);

  const aplicarATodos = () => {
    if (rolApp === "SUPERVISOR") {
      setMetasAsesores((prev) =>
        prev.map((x) => ({ ...x, metaWapeos: aplicarWapeos, metaVentas: aplicarVentas }))
      );
    } else if (rolApp === "ADMIN") {
      setMetasSupervisores((prev) =>
        prev.map((x) => ({ ...x, metaWapeos: aplicarWapeos, metaVentas: aplicarVentas }))
      );
    }
  };

  const guardarFilaAsesor = async (row: MetaAsesorDto) => {
    try {
      const payload: MetaAsesorDto = {
        ...row,
        uunn,
        periodo,
        supervisor: row.supervisor || usuario,
        usuarioAccion: usuario,
        metaWapeos: Number(row.metaWapeos ?? 0),
        metaVentas: Number(row.metaVentas ?? 0),
      };
      await guardarMetaAsesor(payload);
      await Swal.fire("OK", `Meta guardada para ${row.asesor}`, "success");
    } catch (e: any) {
      const msg =
        e?.response?.data?.message ||
        e?.response?.data?.title ||
        JSON.stringify(e?.response?.data) ||
        e?.message ||
        "No se pudo guardar.";
      await Swal.fire("Error", String(msg), "error");
    }
  };

  const guardarFilaSupervisor = async (row: MetaSupervisorDto) => {
    try {
      const payload: MetaSupervisorDto = {
        ...row,
        uunn,
        periodo,
        usuarioAccion: usuario,
        metaWapeos: Number(row.metaWapeos ?? 0),
        metaVentas: Number(row.metaVentas ?? 0),
      };
      await guardarMetaSupervisor(payload);
      await Swal.fire("OK", `Meta guardada para ${row.supervisor}`, "success");
    } catch (e: any) {
      const msg =
        e?.response?.data?.message ||
        e?.response?.data?.title ||
        JSON.stringify(e?.response?.data) ||
        e?.message ||
        "No se pudo guardar.";
      await Swal.fire("Error", String(msg), "error");
    }
  };

  const guardarTodo = async () => {
    try {
      setLoading(true);

      if (rolApp === "SUPERVISOR") {
        for (const row of metasAsesores) {
          await guardarMetaAsesor({
            ...row,
            uunn,
            periodo,
            supervisor: row.supervisor || usuario,
            usuarioAccion: usuario,
            metaWapeos: Number(row.metaWapeos ?? 0),
            metaVentas: Number(row.metaVentas ?? 0),
          });
        }
      } else {
        for (const row of metasSupervisores) {
          await guardarMetaSupervisor({
            ...row,
            uunn,
            periodo,
            usuarioAccion: usuario,
            metaWapeos: Number(row.metaWapeos ?? 0),
            metaVentas: Number(row.metaVentas ?? 0),
          });
        }
      }

      await Swal.fire("Listo", "Metas guardadas correctamente.", "success");
    } catch (e: any) {
      const msg =
        e?.response?.data?.message ||
        e?.response?.data?.title ||
        JSON.stringify(e?.response?.data) ||
        e?.message ||
        "No se pudo guardar.";
      await Swal.fire("Error", String(msg), "error");
    } finally {
      setLoading(false);
      await cargar();
    }
  };

  const titulo = "Panel de metas";
  const subtitulo =
    rolApp === "SUPERVISOR"
      ? "Aquí asignas metas a tus asesores para el mes."
      : "Aquí asignas metas por supervisor para el mes.";

  const esSupervisor = rolApp === "SUPERVISOR";

  return (
    <div className="metas-page">
      <div className="metas-shell">
        <div className="metas-card">
          <header className="metas-head">
            <div className="metas-head-text">
              <h2 className="metas-title">{titulo}</h2>
              <p className="metas-subtitle">{subtitulo}</p>
            </div>
          </header>

          {/* Toolbar estilo “Reportes”: compacto y responsive */}
          <section className="metas-toolbar">
            <div className="metas-grid">
              {rolApp === "ADMIN" && (
                <div className="metas-field metas-span-2">
                  <label className="metas-label">Campaña (UUNN)</label>
                  <input
                    className="metas-input form-control form-control-sm"
                    value={uunn}
                    onChange={(e) => setUunn(e.target.value)}
                    placeholder="Ej: CENCOSUD TC"
                  />
                </div>
              )}

              <div className="metas-field">
                <label className="metas-label">Mes</label>
                <input
                  type="month"
                  className="metas-input form-control form-control-sm"
                  value={monthInputFromYYYYMM(periodo)}
                  onChange={(e) => setPeriodo(yyyymmFromMonthInput(e.target.value))}
                />
              </div>

              <div className="metas-field">
                <label className="metas-label">Buscar</label>
                <input
                  className="metas-input form-control form-control-sm"
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  placeholder={esSupervisor ? "Buscar asesor..." : "Buscar supervisor..."}
                />
              </div>
            </div>

            <div className="metas-actions">
              <button className="btn btn-secondary btn-sm" onClick={cargar} disabled={loading}>
                Recargar
              </button>
              <button className="btn btn-warning btn-sm" onClick={guardarTodo} disabled={loading}>
                Guardar todo
              </button>
            </div>
          </section>

          {/* Aplicar a todos */}
          <section className="metas-bulk">
            <div className="metas-bulk-title">Aplicar metas a todos</div>

            <div className="metas-bulk-grid">
              <div className="metas-field">
                <label className="metas-label">Meta Wapeos</label>
                <input
                  type="number"
                  min={0}
                  className="metas-input form-control form-control-sm"
                  value={aplicarWapeos}
                  onChange={(e) => setAplicarWapeos(Number(e.target.value))}
                />
              </div>

              <div className="metas-field">
                <label className="metas-label">Meta Ventas</label>
                <input
                  type="number"
                  min={0}
                  className="metas-input form-control form-control-sm"
                  value={aplicarVentas}
                  onChange={(e) => setAplicarVentas(Number(e.target.value))}
                />
              </div>
            </div>

            <button className="metas-bulk-btn btn btn-primary btn-sm" onClick={aplicarATodos} disabled={loading}>
              Aplicar
            </button>
          </section>

          {/* Tabla */}
          <section className="metas-table-wrap">
            {loading ? (
              <div className="metas-loading">Cargando...</div>
            ) : esSupervisor ? (
              <div className="table-responsive">
                <table className="table table-dark table-hover align-middle metas-table">
                  <thead>
                    <tr>
                      <th>Asesor</th>
                      <th className="col-meta">Meta Wapeos</th>
                      <th className="col-meta">Meta Ventas</th>
                      <th className="col-accion">Acción</th>
                    </tr>
                  </thead>
                  <tbody>
                    {asesoresFiltrados.map((row, idx) => (
                      <tr key={`${row.asesor}-${idx}`}>
                        <td className="metas-name">{row.asesor}</td>
                        <td>
                          <input
                            type="number"
                            min={0}
                            className="metas-input form-control form-control-sm"
                            value={Number(row.metaWapeos ?? 0)}
                            onChange={(e) => {
                              const v = Number(e.target.value);
                              setMetasAsesores((prev) =>
                                prev.map((x) => (x.asesor === row.asesor ? { ...x, metaWapeos: v } : x))
                              );
                            }}
                          />
                        </td>
                        <td>
                          <input
                            type="number"
                            min={0}
                            className="metas-input form-control form-control-sm"
                            value={Number(row.metaVentas ?? 0)}
                            onChange={(e) => {
                              const v = Number(e.target.value);
                              setMetasAsesores((prev) =>
                                prev.map((x) => (x.asesor === row.asesor ? { ...x, metaVentas: v } : x))
                              );
                            }}
                          />
                        </td>
                        <td>
                          <button className="btn btn-success btn-sm metas-btn-save" onClick={() => guardarFilaAsesor(row)}>
                            Guardar
                          </button>
                        </td>
                      </tr>
                    ))}
                    {asesoresFiltrados.length === 0 && (
                      <tr>
                        <td colSpan={4} className="text-center text-muted">
                          No hay asesores para mostrar.
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            ) : (
              <div className="table-responsive">
                <table className="table table-dark table-hover align-middle metas-table">
                  <thead>
                    <tr>
                      <th>Supervisor</th>
                      <th className="col-meta">Meta Equipo Wapeos</th>
                      <th className="col-meta">Meta Equipo Ventas</th>
                      <th className="col-accion">Acción</th>
                    </tr>
                  </thead>
                  <tbody>
                    {supervisoresFiltrados.map((row, idx) => (
                      <tr key={`${row.supervisor}-${idx}`}>
                        <td className="metas-name">{row.supervisor}</td>
                        <td>
                          <input
                            type="number"
                            min={0}
                            className="metas-input form-control form-control-sm"
                            value={Number(row.metaWapeos ?? 0)}
                            onChange={(e) => {
                              const v = Number(e.target.value);
                              setMetasSupervisores((prev) =>
                                prev.map((x) => (x.supervisor === row.supervisor ? { ...x, metaWapeos: v } : x))
                              );
                            }}
                          />
                        </td>
                        <td>
                          <input
                            type="number"
                            min={0}
                            className="metas-input form-control form-control-sm"
                            value={Number(row.metaVentas ?? 0)}
                            onChange={(e) => {
                              const v = Number(e.target.value);
                              setMetasSupervisores((prev) =>
                                prev.map((x) => (x.supervisor === row.supervisor ? { ...x, metaVentas: v } : x))
                              );
                            }}
                          />
                        </td>
                        <td>
                          <button
                            className="btn btn-success btn-sm metas-btn-save"
                            onClick={() => guardarFilaSupervisor(row)}
                          >
                            Guardar
                          </button>
                        </td>
                      </tr>
                    ))}
                    {supervisoresFiltrados.length === 0 && (
                      <tr>
                        <td colSpan={4} className="text-center text-muted">
                          No hay supervisores para mostrar.
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        </div>
      </div>
    </div>
  );
}
