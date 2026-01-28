// src/pages/ReportesCencosud/ReportesCencosudPage.tsx
import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import dayjs from 'dayjs';
import Swal from 'sweetalert2';

import {
  obtenerResumenMensual,
  obtenerListadoVentas,
  descargarListadoVentasExcel,
  eliminarVenta,
  obtenerAsesoresSupervisor, // ✅ NUEVO
  type ListadoVentasParams,
} from '../../services/cencosud/reportes.service';

import type {
  CencosudResumenMensualItem,
  CencosudVentaListadoItem,
} from '../../types/reportes';

import './ReportesCencosudPage.css';

const ESTADOS_RESUMEN = ['VENTA', 'COORDINADO', 'SIN OFERTA'];

function ReportesCencosudPage() {
  // ================= DATOS DE USUARIO =================
  const cargoRaw = localStorage.getItem('cargo') || '';
  const cargo = cargoRaw.toUpperCase();
  const navigate = useNavigate();

  const rolAppLocal = (localStorage.getItem('rolApp') || '')
    .trim()
    .toUpperCase();

  // Rol principal: ASESOR / SUPERVISOR / ADMIN
  let rolApp: 'ASESOR' | 'SUPERVISOR' | 'ADMIN';

  if (
    rolAppLocal === 'ASESOR' ||
    rolAppLocal === 'SUPERVISOR' ||
    rolAppLocal === 'ADMIN'
  ) {
    rolApp = rolAppLocal as 'ASESOR' | 'SUPERVISOR' | 'ADMIN';
  } else {
    // Fallback por cargo
    if (cargo.includes('PROMOTOR')) {
      rolApp = 'ASESOR';
    } else if (cargo.includes('SUPERVISOR')) {
      rolApp = 'SUPERVISOR';
    } else {
      rolApp = 'ADMIN';
    }
  }

  // ================= URL PARAMS (persistir filtros) =================
  const [searchParams, setSearchParams] = useSearchParams();
  const qp = (k: string) => (searchParams.get(k) || '').trim();

  // ================= ESTADOS =================
  // Filtros (inicializa leyendo querystring)
  const [fechaIni, setFechaIni] = useState<string>(
    qp('fechaIni') || dayjs().startOf('month').format('YYYY-MM-DD')
  );
  const [fechaFin, setFechaFin] = useState<string>(
    qp('fechaFin') || dayjs().endOf('month').format('YYYY-MM-DD')
  );
  const [estado, setEstado] = useState<string>(qp('estado') || '');
  const [dniCliente, setDniCliente] = useState<string>(qp('dni') || '');

  // ✅ NUEVO: filtro por vendedor (asesor)
  const [vendedor, setVendedor] = useState<string>(qp('vendedor') || '');

  const [page, setPage] = useState<number>(Number(qp('page')) || 1);
  const [pageSize, setPageSize] = useState<number>(Number(qp('pageSize')) || 20);

  // ✅ NUEVO: debounce para DNI (LIKE mientras escribes sin spamear)
  const [dniDebounced, setDniDebounced] = useState<string>(dniCliente);

  useEffect(() => {
    const t = window.setTimeout(() => {
      setDniDebounced(dniCliente.trim());
    }, 350);

    return () => window.clearTimeout(t);
  }, [dniCliente]);

  // ✅ NUEVO: asesores del supervisor
  const [asesores, setAsesores] = useState<string[]>([]);
  const [cargandoAsesores, setCargandoAsesores] = useState(false);

  useEffect(() => {
    // Solo SUPERVISOR necesita combo de asesores
    if (rolApp !== 'SUPERVISOR') {
      setAsesores([]);
      setVendedor(''); // evitar filtros raros si cambias rol
      return;
    }

    const uunn = (localStorage.getItem('uunn') || '').trim() || undefined;

    (async () => {
      try {
        setCargandoAsesores(true);
        const data = await obtenerAsesoresSupervisor(uunn);
        setAsesores(data || []);
      } catch (e) {
        console.error(e);
        setAsesores([]);
      } finally {
        setCargandoAsesores(false);
      }
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [rolApp]);

  // 🔒 Cada vez que cambie un filtro, se guarda en la URL
  useEffect(() => {
    const next = new URLSearchParams();

    next.set('fechaIni', fechaIni);
    next.set('fechaFin', fechaFin);

    if (estado) next.set('estado', estado);
    if (dniCliente) next.set('dni', dniCliente);

    // ✅ NUEVO
    if (rolApp === 'SUPERVISOR' && vendedor) next.set('vendedor', vendedor);

    next.set('page', String(page));
    next.set('pageSize', String(pageSize));

    setSearchParams(next, { replace: true });
  }, [
    fechaIni,
    fechaFin,
    estado,
    dniCliente,
    vendedor,
    page,
    pageSize,
    rolApp,
    setSearchParams,
  ]);

  // Datos
  const [resumen, setResumen] = useState<CencosudResumenMensualItem[]>([]);
  const [registros, setRegistros] = useState<CencosudVentaListadoItem[]>([]);
  const [totalRegistros, setTotalRegistros] = useState<number>(0);

  // UI
  const [cargandoResumen, setCargandoResumen] = useState(false);
  const [cargandoListado, setCargandoListado] = useState(false);
  const [descargandoExcel, setDescargandoExcel] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Paginación (tu lógica)
  const totalConocido = totalRegistros > 0;
  const puedeAnterior = page > 1;
  const puedeSiguiente = registros.length === pageSize;

  const totalParaMostrar = totalConocido
    ? totalRegistros
    : (page - 1) * pageSize + registros.length;

  const inicio = totalParaMostrar === 0 ? 0 : (page - 1) * pageSize + 1;
  const fin = totalParaMostrar === 0 ? 0 : (page - 1) * pageSize + registros.length;

  // ================= RESUMEN NORMALIZADO =================
  const resumenNormalizado = ESTADOS_RESUMEN.map((estadoBase) => {
    const item = resumen.find((r) => (r.estado || '').toUpperCase() === estadoBase);
    return { estado: estadoBase, total: item?.cantidad ?? 0 };
  });

  const totalResumen = resumenNormalizado.reduce((acc, item) => acc + item.total, 0);
  const hayDatosResumen = totalResumen > 0;

  // ================= CARGAR RESUMEN =================
  const cargarResumen = async () => {
    try {
      setCargandoResumen(true);
      setError(null);
      const data = await obtenerResumenMensual(fechaIni, fechaFin);
      setResumen(data);
    } catch (err) {
      console.error(err);
      setError('No se pudo cargar el resumen mensual.');
    } finally {
      setCargandoResumen(false);
    }
  };

  useEffect(() => {
    cargarResumen();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [fechaIni, fechaFin]);

  // ================= CARGAR LISTADO =================
  const cargarListado = async () => {
    try {
      setCargandoListado(true);
      setError(null);

      const params: ListadoVentasParams = {
        fechaIni,
        fechaFin,
        estado: estado || undefined,
        dniCliente: dniDebounced || undefined, // ✅ usa debounce
        vendedor: rolApp === 'SUPERVISOR' ? (vendedor || undefined) : undefined, // ✅
        page,
        pageSize,
      };

      const data = await obtenerListadoVentas(params);

      setRegistros(data.registros || []);

      const total =
        (data as any).totalRegistros ??
        (data as any).totalFilas ??
        (Array.isArray(data.registros) ? data.registros.length : 0);

      setTotalRegistros(total);
    } catch (err) {
      console.error(err);
      setError('No se pudo cargar el listado de ventas.');
    } finally {
      setCargandoListado(false);
    }
  };

  useEffect(() => {
    cargarListado();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [fechaIni, fechaFin, estado, dniDebounced, vendedor, page, pageSize, rolApp]);

  // ================= TEXTOS SEGÚN ROL =================
  const titulo =
    rolApp === 'SUPERVISOR'
      ? 'Reportes y seguimiento'
      : rolApp === 'ADMIN'
      ? 'Reporte general Cencosud'
      : 'Mi resumen de gestiones';

  const subtitulo =
    rolApp === 'SUPERVISOR'
      ? 'Revisa las ventas de tus asesores en el periodo seleccionado.'
      : rolApp === 'ADMIN'
      ? 'Visualiza el comportamiento global de todas las ventas.'
      : 'Mira tus propias ventas, coordinaciones y estados del mes.';

  // ================= EXPORTAR EXCEL =================
  const handleExportarExcel = async () => {
    try {
      setDescargandoExcel(true);

      const params: ListadoVentasParams = {
        fechaIni,
        fechaFin,
        estado: estado || undefined,
        dniCliente: dniCliente || undefined, // para export no necesitas debounce
        vendedor: rolApp === 'SUPERVISOR' ? (vendedor || undefined) : undefined, // ✅
      };

      await descargarListadoVentasExcel(params);
    } catch (err) {
      console.error(err);
      setError('No se pudo descargar el Excel de ventas.');
    } finally {
      setDescargandoExcel(false);
    }
  };

  // ================= EDITAR / ELIMINAR =================
  const handleEditar = (item: CencosudVentaListadoItem) => {
    const returnTo = `/reportes?${searchParams.toString()}`;
    navigate(
      `/registro?idCliente=${item.idCliente}&returnTo=${encodeURIComponent(returnTo)}`
    );
  };

  const handleEliminar = async (item: CencosudVentaListadoItem) => {
    if (rolApp !== 'ADMIN') return;

    const result = await Swal.fire({
      title: 'Eliminar venta',
      text: `¿Seguro que deseas eliminar la venta del DNI ${item.dniCliente}?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#d33',
      cancelButtonColor: '#6c757d',
      reverseButtons: true,
    });

    if (!result.isConfirmed) return;

    try {
      setCargandoListado(true);
      await eliminarVenta(item.idCliente);

      await Swal.fire({
        icon: 'success',
        title: 'Venta eliminada',
        text: 'La venta se eliminó correctamente.',
        confirmButtonColor: '#3085d6',
      });

      setPage(1);
      await cargarListado();
      await cargarResumen();
    } catch (err: any) {
      console.error(err);

      const msgBackend =
        err?.response?.data?.mensaje ||
        err?.response?.data?.Mensaje ||
        'No se pudo eliminar la venta.';

      setError(msgBackend);

      await Swal.fire({
        icon: 'error',
        title: 'Error',
        text: msgBackend,
        confirmButtonColor: '#d33',
      });
    } finally {
      setCargandoListado(false);
    }
  };

  const hayRegistrosTabla = registros.length > 0;

  // ================= RENDER =================
  return (
    <div className="rep-page">
      <div className="rep-shell">
        <div className="rep-panel">
          <div className="rep-header">
            <h3 className="rep-title">{titulo}</h3>
            <p className="rep-subtitle">{subtitulo}</p>
          </div>

          {error && <div className="rep-alert">{error}</div>}

          {/* RESUMEN */}
          <div className="rep-card">
            <div className="rep-card-body">
              <div className="rep-date-row">
                <div className="rep-field">
                  <label className="rep-label">Desde</label>
                  <input
                    type="date"
                    className="rep-input"
                    value={fechaIni}
                    onChange={(e) => {
                      setFechaIni(e.target.value);
                      setPage(1);
                    }}
                  />
                </div>

                <div className="rep-field">
                  <label className="rep-label">Hasta</label>
                  <input
                    type="date"
                    className="rep-input"
                    value={fechaFin}
                    onChange={(e) => {
                      setFechaFin(e.target.value);
                      setPage(1);
                    }}
                  />
                </div>
              </div>

              {cargandoResumen ? (
                <p className="rep-muted">Cargando resumen...</p>
              ) : (
                <>
                  {!hayDatosResumen && (
                    <p className="rep-muted rep-muted-small">
                      No hay datos para el periodo seleccionado.
                    </p>
                  )}

                  <div className="rep-summary-grid">
                    {resumenNormalizado.map((item) => (
                      <div className="rep-summary-box" key={item.estado}>
                        <div className="rep-summary-label">{item.estado}</div>
                        <div className="rep-summary-value">{item.total}</div>
                      </div>
                    ))}
                  </div>
                </>
              )}
            </div>
          </div>

          {/* LISTADO */}
          <div className="rep-card">
            <div className="rep-card-body">
              <div className="rep-list-head">
                <div>
                  <h5 className="rep-card-title">Listado de ventas</h5>
                </div>

                <button
                  type="button"
                  className="rep-btn rep-btn-success"
                  onClick={handleExportarExcel}
                  disabled={descargandoExcel || !hayRegistrosTabla}
                >
                  {descargandoExcel ? 'Generando Excel...' : 'Descargar Excel'}
                </button>
              </div>

              <div className="rep-filters">
                <div className="rep-field">
                  <label className="rep-label">Estado</label>
                  <select
                    className="rep-select"
                    value={estado}
                    onChange={(e) => {
                      setEstado(e.target.value);
                      setPage(1);
                    }}
                  >
                    <option value="">Todos</option>
                    <option value="VENTA">VENTA</option>
                    <option value="COORDINADO">COORDINADO</option>
                    <option value="SIN OFERTA">SIN OFERTA</option>
                  </select>
                </div>

                {/* ✅ NUEVO: combo asesores (solo supervisor) */}
                {rolApp === 'SUPERVISOR' && (
                  <div className="rep-field">
                    <label className="rep-label">Asesor</label>
                    <select
                      className="rep-select"
                      value={vendedor}
                      onChange={(e) => {
                        setVendedor(e.target.value);
                        setPage(1);
                      }}
                      disabled={cargandoAsesores}
                    >
                      <option value="">Todos</option>
                      {asesores.map((a) => (
                        <option key={a} value={a}>
                          {a}
                        </option>
                      ))}
                    </select>
                  </div>
                )}

                <div className="rep-field">
                  <label className="rep-label">DNI</label>
                  <input
                    type="text"
                    maxLength={8}
                    className="rep-input"
                    value={dniCliente}
                    onChange={(e) => {
                      setDniCliente(e.target.value);
                      setPage(1);
                    }}
                  />
                </div>

                <div className="rep-field rep-field-small">
                  <label className="rep-label">Registros</label>
                  <select
                    className="rep-select"
                    value={pageSize}
                    onChange={(e) => {
                      setPageSize(Number(e.target.value));
                      setPage(1);
                    }}
                  >
                    <option value={20}>20</option>
                    <option value={50}>50</option>
                    <option value={100}>100</option>
                  </select>
                </div>
              </div>

              {cargandoListado ? (
                <p className="rep-muted">Cargando registros...</p>
              ) : registros.length === 0 ? (
                <p className="rep-muted">
                  No se encontraron registros con los filtros actuales.
                </p>
              ) : (
                <>
                  <div className="rep-table-wrap">
                    <table className="rep-table">
                      <thead>
                        <tr>
                          <th>Fecha</th>
                          <th>DNI</th>
                          <th>Estado</th>
                          {rolApp !== 'ASESOR' && <th>Vendedor</th>}
                          <th className="rep-th-actions">Acciones</th>
                        </tr>
                      </thead>

                      <tbody>
                        {registros.map((r) => (
                          <tr key={r.idCliente}>
                            <td>{r.dateEntry?.substring(0, 10)}</td>
                            <td>{r.dniCliente}</td>
                            <td>{r.estado ?? '-'}</td>
                            {rolApp !== 'ASESOR' && (
                              <td>{r.nomVendedor ?? '-'}</td>
                            )}
                            <td className="rep-actions">
                              <button
                                type="button"
                                className="rep-btn-mini rep-btn-warn"
                                onClick={() => handleEditar(r)}
                              >
                                Editar
                              </button>

                              <button
                                type="button"
                                className="rep-btn-mini rep-btn-danger"
                                onClick={() => handleEliminar(r)}
                                disabled={rolApp !== 'ADMIN'}
                                title={
                                  rolApp !== 'ADMIN'
                                    ? 'Solo el administrador puede eliminar'
                                    : 'Eliminar venta'
                                }
                              >
                                Eliminar
                              </button>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>

                  <div className="rep-pagination">
                    <div className="rep-muted rep-muted-small">
                      Mostrando {inicio} - {fin} de{' '}
                      {totalConocido ? totalRegistros : `${totalParaMostrar}+`} registros
                    </div>

                    <div className="rep-pager">
                      <button
                        type="button"
                        className="rep-pager-btn"
                        disabled={!puedeAnterior}
                        onClick={() => setPage((p) => Math.max(1, p - 1))}
                      >
                        «Anterior
                      </button>

                      <button
                        type="button"
                        className="rep-pager-btn"
                        disabled={!puedeSiguiente}
                        onClick={() => setPage((p) => p + 1)}
                      >
                        Siguiente»
                      </button>
                    </div>
                  </div>
                </>
              )}
            </div>
          </div>

        </div>
      </div>
    </div>
  );
}

export default ReportesCencosudPage;
