import { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import Swal from 'sweetalert2';

import {
  guardarClienteCencosud,
  obtenerClientePorId,
} from '../../services/cencosud/tienda.service';

import { leerImagenCencosudIA } from '../../services/ocr/ocr.service';

import type { CencosudClienteDetalle } from '../../types/cencosud';
import './RegistroCencosudPage.css';

function RegistroCencosudPage() {
  const navigate = useNavigate();
  const location = useLocation();

  // =============== MODO EDICIÓN ===============
  const searchParams = new URLSearchParams(location.search);
  const idClienteParam = searchParams.get('idCliente');
  const idClienteEdicion = idClienteParam ? Number(idClienteParam) : null;
  const esEdicion = idClienteEdicion !== null;

  const [archivo, setArchivo] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);

  const [dni, setDni] = useState('');
  const [nombre, setNombre] = useState('');
  const [telefono, setTelefono] = useState('');
  const [estado, setEstado] = useState('');
  const [comentario, setComentario] = useState('');
  const [dynamicFields, setDynamicFields] = useState<Record<string, string>>({});
  const [procesandoOcr, setProcesandoOcr] = useState(false);
  const [guardando, setGuardando] = useState(false);
  const [showModal, setShowModal] = useState(false);

  // Opcional (por si luego lo conectas con backend)
  const [tieneImagenActual, setTieneImagenActual] = useState<boolean>(false);

  // ====== Limpieza de URL.createObjectURL para evitar leaks ======
  useEffect(() => {
    return () => {
      if (preview) URL.revokeObjectURL(preview);
    };
  }, [preview]);

  // ====== Cargar datos cuando es EDICIÓN ======
  useEffect(() => {
    const cargarDatosEdicion = async () => {
      if (!idClienteEdicion) return;

      try {
        const detalle: CencosudClienteDetalle =
          await obtenerClientePorId(idClienteEdicion);

        setDni(detalle.dniCliente);
        setNombre(detalle.nombreCliente);
        setTelefono(detalle.telefono ?? '');
        setEstado(detalle.estado ?? '');
        setComentario(detalle.comentario ?? '');

        const dinamicos: Record<string, string> = {};

        // campos fijos que ya tenías
        if (detalle.tipoTramite) dinamicos['Tipo de Trámite'] = detalle.tipoTramite;
        if (detalle.oferta) dinamicos['Oferta'] = detalle.oferta;
        if (detalle.incrementoLinea) dinamicos['Incremento de Línea'] = detalle.incrementoLinea;
        if (detalle.avanceEfectivo) dinamicos['Avance Efectivo'] = detalle.avanceEfectivo;
        if (detalle.superavance) dinamicos['Superavance'] = detalle.superavance;
        if (detalle.cambioProducto) dinamicos['Cambio de Producto'] = detalle.cambioProducto;

        // nuevos campos que definimos en CencosudClienteDetalle
        if (detalle.adicionales) dinamicos['Adicionales'] = detalle.adicionales;
        if (detalle.efectivoCencosud) dinamicos['Efectivo Cencosud'] = detalle.efectivoCencosud;
        if (detalle.ecPct) dinamicos['EC Pct'] = detalle.ecPct;
        if (detalle.ecTasa) dinamicos['EC Tasa'] = detalle.ecTasa;
        if (detalle.aePct) dinamicos['AE Pct'] = detalle.aePct;
        if (detalle.aeTasa) dinamicos['AE Tasa'] = detalle.aeTasa;
        
        if (detalle.infoAdicional) {
          Object.entries(detalle.infoAdicional).forEach(([clave, valor]) => {
            // solo escribe si el valor está definido
            if (valor) dinamicos[clave] = valor;
          });
        }

        setDynamicFields(dinamicos);

        // Si luego tu backend envía un flag, aquí lo puedes setear:
        // setTieneImagenActual(!!detalle.tieneImagen);

        // En edición abrimos directamente el modal
        setShowModal(true);
      } catch (err) {
        console.error(err);
        await Swal.fire('Error', 'No se pudo cargar la venta para editar.', 'error');
        navigate(-1);
      }
    };

    if (esEdicion) cargarDatosEdicion();
  }, [esEdicion, idClienteEdicion, navigate]);

  // --- Detectar imagen y leer con IA ---
  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] ?? null;

    // Limpia preview anterior
    if (preview) URL.revokeObjectURL(preview);

    setArchivo(file);
    setPreview(file ? URL.createObjectURL(file) : null);
    if (!file) return;

    try {
      setProcesandoOcr(true);

      const campos = await leerImagenCencosudIA(file);

      if (campos.dni) setDni(campos.dni);
      if (campos.nombre) setNombre(campos.nombre);

      const dinamicos: Record<string, string> = {};
      if (campos.tipo_tramite) dinamicos['Tipo de Trámite'] = campos.tipo_tramite;
      if (campos.oferta) dinamicos['Oferta'] = campos.oferta;
      if (campos.incremento_de_linea) dinamicos['Incremento de Línea'] = campos.incremento_de_linea;
      if (campos.avance_efectivo) dinamicos['Avance Efectivo'] = campos.avance_efectivo;
      // nuevos:
      if (campos.adicionales) dinamicos['Adicionales'] = campos.adicionales;
      if (campos.efectivo_cencosud) dinamicos['Efectivo Cencosud'] = campos.efectivo_cencosud;
      if (campos.ec_pct) dinamicos['EC Pct'] = campos.ec_pct;
      if (campos.ec_tasa) dinamicos['EC Tasa'] = campos.ec_tasa;
      if (campos.ae_pct) dinamicos['AE Pct'] = campos.ae_pct;
      if (campos.ae_tasa) dinamicos['AE Tasa'] = campos.ae_tasa;
      if (campos.hora_wapeo) dinamicos['HORA_WAPEO'] = campos.hora_wapeo;
      
      setDynamicFields(dinamicos);
    } catch (err) {
      console.error(err);
      Swal.fire('Error', 'No se pudo leer la imagen.', 'error');
    } finally {
      setProcesandoOcr(false);
    }
  };

  // --- Validaciones básicas ---
  const validarFormulario = (): boolean => {
    if (!dni || dni.length !== 8) {
      Swal.fire('DNI inválido', 'Debe ingresar un DNI de 8 dígitos.', 'warning');
      return false;
    }
    if (telefono.trim() !== '' && !/^\d{9}$/.test(telefono.trim())) {
      Swal.fire(
        'Teléfono inválido',
        'Si ingresa teléfono, debe tener exactamente 9 dígitos.',
        'warning'
      );
      return false;
    }
    if (!nombre.trim()) {
      Swal.fire('Campo vacío', 'Debe ingresar el nombre del cliente.', 'warning');
      return false;
    }
    if (!estado) {
      Swal.fire('Estado requerido', 'Debe seleccionar un estado.', 'warning');
      return false;
    }
    return true;
  };

  // --- Cerrar modal ---
  const handleCerrarModal = () => {
    setShowModal(false);
    if (esEdicion) navigate(-1);
  };

  // --- Guardar datos (alta o edición) ---
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validarFormulario()) return;

    try {
      setGuardando(true);

      const cargo = localStorage.getItem('cargo');
      const supervisor = localStorage.getItem('supervisor');
      const uunn = localStorage.getItem('uunn');
      const nomVendedor = localStorage.getItem('usuario');

      const payload = {
        idCliente: esEdicion ? idClienteEdicion : null,
        dniCliente: dni,
        nombreCliente: nombre,
        telefono,
        estado,
        comentario,
        usuarioCencosud: null,
        cargo,
        supervisor,
        uunn,
        nomVendedor,
        infoAdicional: dynamicFields,
        tieneImagen: archivo ? true : tieneImagenActual,
      };

      const resp = await guardarClienteCencosud(payload);

      if (resp.codigoResultado === 0) {
        if (esEdicion) {
          await Swal.fire({
            icon: 'success',
            title: 'Cambios guardados',
            text: 'La venta se actualizó correctamente.',
            confirmButtonColor: '#3085d6',
          });
          setShowModal(false);
          navigate(-1);
          return;
        }

        await Swal.fire({
          icon: 'success',
          title: '¡Registro guardado!',
          text: 'Se guardó correctamente el cliente.',
          confirmButtonColor: '#3085d6',
        });

        // Limpiar formulario en alta
        setArchivo(null);
        setPreview(null);
        setDni('');
        setNombre('');
        setTelefono('');
        setEstado('');
        setComentario('');
        setDynamicFields({});
        setTieneImagenActual(false);
        setShowModal(false);
      } else if (resp.codigoResultado === 1001) {
        Swal.fire({
          icon: 'warning',
          title: 'Registro duplicado',
          text: resp.mensaje,
          confirmButtonColor: '#f8bb86',
        });
      } else {
        Swal.fire({
          icon: 'error',
          title: 'Error al guardar',
          text: resp.mensaje || 'Ocurrió un error al guardar.',
          confirmButtonColor: '#d33',
        });
      }
    } catch (err) {
      console.error(err);
      Swal.fire('Error', 'Error de conexión con el servidor.', 'error');
    } finally {
      setGuardando(false);
    }
  };

  // ====== Textos según modo ======
  const tituloModal = esEdicion ? 'Edición de venta' : 'Formulario de venta';
  const textoBotonSubmit = esEdicion ? 'Guardar cambios' : 'Guardar registro';

  return (
    <div className="registro-page">
      <div className="registro-panel">
        <div className="registro-shell">
          {/* Solo mostramos la tarjeta de subida de imagen cuando NO es edición */}
          {!esEdicion && (
            <div className="card border-0 mb-4 registro-card">
              <div className="card-body text-center">
                <div className={`registro-upload-frame ${procesandoOcr ? 'is-loading' : ''}`}>
                  {preview ? (
                    <img src={preview} alt="preview" className="registro-upload-img" />
                  ) : (
                    <span className="registro-upload-placeholder">
                      Previsualización de la imagen
                    </span>
                  )}
                </div>

                <div className="mt-3 d-flex flex-column gap-2">
                  {/* File picker bonito */}
                  <div className="registro-file-row">
                    <label className="registro-file-btn">
                      Elegir archivo
                      <input
                        type="file"
                        accept="image/*"
                        onChange={handleFileChange}
                      />
                    </label>

                    <div className="registro-file-name">
                      {archivo ? archivo.name : 'No se ha seleccionado archivo'}
                    </div>
                  </div>

                  <button
                    type="button"
                    className="registro-btn registro-btn-primary"
                    disabled={!archivo || procesandoOcr}
                    onClick={() => setShowModal(true)}
                  >
                    {procesandoOcr ? 'Leyendo imagen...' : 'Revisar datos y guardar'}
                  </button>

                  <button
                    type="button"
                    className="registro-btn registro-btn-secondary"
                    onClick={() => navigate('/reportes')}
                  >
                    Ir a mis registros
                  </button>
                </div>

                <p className="mt-2 text-muted registro-help-text">
                  1) Sube la captura del wapeo.&nbsp;
                  2) Pulsa <strong>“Revisar datos y guardar”</strong> para confirmar.
                </p>
              </div>
            </div>
          )}

          {/* En edición, el modal se abre automáticamente con los datos cargados */}
          {showModal && (
            <>
              <div className="modal fade show d-block" tabIndex={-1}>
                <div className="modal-dialog modal-lg modal-dialog-centered">
                  <div className="modal-content registro-modal">
                    <div className="modal-header">
                      <h5 className="modal-title">{tituloModal}</h5>
                      <button
                        type="button"
                        className="btn-close"
                        onClick={handleCerrarModal}
                      ></button>
                    </div>

                    <div className="modal-body">
                      <form onSubmit={handleSubmit} className="registro-form">
                        <div className="mb-3">
                          <label className="form-label">DNI Cliente</label>
                          <input
                            className="form-control"
                            value={dni}
                            onChange={(e) => setDni(e.target.value)}
                            maxLength={8}
                          />
                        </div>

                        <div className="mb-3">
                          <label className="form-label">Nombre Cliente</label>
                          <input
                            className="form-control"
                            value={nombre}
                            onChange={(e) => setNombre(e.target.value)}
                          />
                        </div>

                        <h6 className="mt-3 mb-2">Datos de venta</h6>
                        {Object.entries(dynamicFields).map(([key, value]) => (
                          <div className="row g-2 mb-1" key={key}>
                            <div className="col-5">
                              <input
                                type="text"
                                className="form-control form-control-sm"
                                value={key}
                                readOnly
                              />
                            </div>
                            <div className="col-7">
                              <input
                                type="text"
                                className="form-control form-control-sm"
                                value={value}
                                onChange={(e) =>
                                  setDynamicFields((prev) => ({
                                    ...prev,
                                    [key]: e.target.value,
                                  }))
                                }
                              />
                            </div>
                          </div>
                        ))}

                        <div className="mb-3 mt-3">
                          <label className="form-label">Teléfono</label>
                          <input
                            className="form-control"
                            value={telefono}
                            onChange={(e) => setTelefono(e.target.value)}
                          />
                        </div>

                        <div className="mb-3">
                          <label className="form-label">Estado</label>
                          <select
                            className="form-select"
                            value={estado}
                            onChange={(e) => setEstado(e.target.value)}
                          >
                            <option value="">-- Seleccionar --</option>
                            <option value="VENTA">VENTA</option>
                            <option value="COORDINADO">COORDINADO</option>
                            <option value="SIN OFERTA">SIN OFERTA</option>
                          </select>
                        </div>

                        <div className="mb-3">
                          <label className="form-label">Comentario</label>
                          <textarea
                            className="form-control"
                            rows={3}
                            value={comentario}
                            onChange={(e) => setComentario(e.target.value)}
                          />
                        </div>

                        <div className="d-flex justify-content-end gap-2">
                          <button
                            type="button"
                            className="btn btn-secondary"
                            onClick={handleCerrarModal}
                            disabled={guardando}
                          >
                            Cancelar
                          </button>

                          <button
                            type="submit"
                            className="btn btn-primary"
                            disabled={guardando}
                          >
                            {guardando ? 'Guardando...' : textoBotonSubmit}
                          </button>
                        </div>
                      </form>
                    </div>
                  </div>
                </div>
              </div>

              <div className="modal-backdrop fade show"></div>
            </>
          )}
        </div>
      </div>
    </div>
  );
}

export default RegistroCencosudPage;
