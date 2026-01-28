import { useState } from "react";
import type { FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { loginApi } from "../../services/auth/auth.service";
import "./LoginPage.css";
import { useAuth } from "../../hooks/useAuth";

const LOGO_URL =
  "https://www.tarjetacencosud.pe/wp-content/themes/annariel-cencosud2024/assets/ce413761e234a3c6843f.png";

function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuth();

  const [usuario, setUsuario] = useState("");
  const [password, setPassword] = useState("");
  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [mostrarPass, setMostrarPass] = useState(false);

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);
    setCargando(true);

    try {
      const resp = await loginApi({ usuario, password });

      if (!resp.token) {
        throw new Error("Token inválido en la respuesta del servidor.");
      }

      login(resp);

      navigate("/dashboard", { replace: true });
    } catch (err) {
      console.error(err);
      setError("Usuario o contraseña incorrectos.");
    } finally {
      setCargando(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-shell">
        <div className="login-layout">
          {/* Panel informativo: aparece en desktop para que el login no se vea “chico” */}
          <section className="login-hero" aria-label="Información del sistema">
            <div className="login-heroBadge">CENCO SYSTEM</div>
            <h2 className="login-heroTitle">Gestión de ventas, simple y rápido</h2>
            <p className="login-heroText">
              Inicia sesión para registrar ventas, procesar documentos con OCR y
              acceder a reportes según tu rol.
            </p>

            <ul className="login-heroList">
              <li>Registro de ventas con imagen/OCR</li>
              <li>Reportes por asesor y supervisor</li>
              <li>Accesos controlados por roles</li>
            </ul>

          </section>

          <div className="login-card">
            <div className="login-brand">
              <img className="login-logo" src={LOGO_URL} alt="Cencosud" />
            </div>

            <h1 className="login-title">BIENVENIDO</h1>
            <p className="login-subtitle">
              Ingresa con tu usuario de ALMPES
              <br />
              para continuar
            </p>

            {error && (
              <div className="login-alert" role="alert">
                {error}
              </div>
            )}

            <form className="login-form" onSubmit={handleSubmit}>
              <label className="login-label" htmlFor="usuario">
                Usuario
              </label>
              <div className="login-field">
                <span className="login-icon" aria-hidden="true">
                  <svg viewBox="0 0 24 24" className="login-iconSvg">
                    <path
                      d="M12 12a4.5 4.5 0 1 0-4.5-4.5A4.5 4.5 0 0 0 12 12Zm0 2.25c-4.2 0-7.5 2.1-7.5 4.5V20h15v-1.25c0-2.4-3.3-4.5-7.5-4.5Z"
                      fill="currentColor"
                    />
                  </svg>
                </span>

                <input
                  id="usuario"
                  type="text"
                  className="login-input login-input--with-icon"
                  placeholder="Ej: NOMBRE.APELLIDO"
                  value={usuario}
                  onChange={(e) => setUsuario(e.target.value)}
                  autoComplete="username"
                />
              </div>

              <label className="login-label" htmlFor="password">
                Contraseña
              </label>
              <div className="login-field">
                <span className="login-icon" aria-hidden="true">
                  <svg viewBox="0 0 24 24" className="login-iconSvg">
                    <path
                      d="M17 10V8a5 5 0 0 0-10 0v2H6a2 2 0 0 0-2 2v7a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-7a2 2 0 0 0-2-2Zm-8 0V8a3 3 0 0 1 6 0v2Z"
                      fill="currentColor"
                    />
                  </svg>
                </span>

                <input
                  id="password"
                  type={mostrarPass ? "text" : "password"}
                  className="login-input login-input--with-icon login-input--with-action"
                  placeholder="Contraseña"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  autoComplete="current-password"
                />

                <button
                  type="button"
                  className="login-eye"
                  onClick={() => setMostrarPass((v) => !v)}
                  aria-label={mostrarPass ? "Ocultar contraseña" : "Mostrar contraseña"}
                  title={mostrarPass ? "Ocultar" : "Mostrar"}
                >
                  <svg viewBox="0 0 24 24" className="login-eyeSvg">
                    <path
                      d="M12 5c5.5 0 9.6 4.4 10.7 6-.9 1.6-5.2 8-10.7 8S2.4 12.6 1.3 11C2.4 9.4 6.5 5 12 5Zm0 2C8 7 4.7 9.9 3.5 11 4.7 12.1 8 17 12 17s7.3-4.9 8.5-6C19.3 9.9 16 7 12 7Zm0 2.2A3.8 3.8 0 1 1 8.2 13 3.8 3.8 0 0 1 12 9.2Zm0 2A1.8 1.8 0 1 0 13.8 13 1.8 1.8 0 0 0 12 11.2Z"
                      fill="currentColor"
                    />
                  </svg>
                </button>
              </div>

              <button type="submit" className="login-btn" disabled={cargando}>
                {cargando ? "Ingresando..." : "Ingresar"}
              </button>
            </form>

            <a className="login-forgot" href="#" onClick={(e) => e.preventDefault()}>
              ¿Olvidaste tu contraseña?
            </a>
          </div>
        </div>

        <p className="login-footer">© CLEINER TAFUR - TODOS LOS DERECHOS RESERVADOS.</p>
      </div>
    </div>
  );
}

export default LoginPage;
