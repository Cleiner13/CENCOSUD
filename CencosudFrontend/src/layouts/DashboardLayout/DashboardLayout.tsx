// src/layouts/DashboardLayout/DashboardLayout.tsx
import type { ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';
import './DashboardLayout.css';
import { useAuth } from '../../hooks/useAuth';

type DashboardLayoutProps = {
  children: ReactNode;
};

function DashboardLayout({ children }: DashboardLayoutProps) {
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  const usuario = user?.usuario ?? '';
  const cargo = user?.cargo ?? '';

  // 🔔 Estado para mostrar el modal de sesión expirada
  const [sessionExpired, setSessionExpired] = useState(false);

  useEffect(() => {
    const handler = () => setSessionExpired(true);

    window.addEventListener('session-expired', handler);
    return () => window.removeEventListener('session-expired', handler);
  }, []);

  const handleLogout = () => {
    logout();
    navigate('/login', { replace: true });
  };

  const handleGoLoginFromModal = () => {
    setSessionExpired(false);
    navigate('/login', { replace: true });
  };

  return (
    <div className="dashboard-shell">
      {/* Topbar tipo “pastilla” */}
      <header className="cenco-topbar-wrapper">
        <div className="cenco-topbar">
          <div className="cenco-topbar-left">
            <span
              className="cenco-logo-text"
              onClick={() => navigate('/dashboard')}
              style={{ cursor: 'pointer' }}
            >
              CENCOSUD
            </span>
          </div>
          <div className="cenco-user-info">
            <div className="cenco-user-name">{usuario || 'USUARIO'}</div>
            <div className="cenco-user-role">{cargo || '—'}</div>
          </div>
          <button
            className="cenco-logout-btn"
            type="button"
            onClick={handleLogout}
          >
            Cerrar sesión
          </button>
        </div>
      </header>

      {/* 🌟 Modal bonito de sesión expirada */}
      {sessionExpired && (
        <>
          <div className="modal fade show d-block" tabIndex={-1}>
            <div className="modal-dialog modal-dialog-centered">
              <div className="modal-content">
                <div className="modal-header">
                  <h5 className="modal-title">Sesión expirada</h5>
                  <button
                    type="button"
                    className="btn-close"
                    onClick={handleGoLoginFromModal}
                  ></button>
                </div>
                <div className="modal-body">
                  <p className="mb-0">
                    Tu sesión ha expirado por inactividad. 
                    Vuelve a iniciar sesión para continuar usando el sistema.
                  </p>
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-primary"
                    onClick={handleGoLoginFromModal}
                  >
                    Ir al inicio de sesión
                  </button>
                </div>
              </div>
            </div>
          </div>
          {/* Fondo oscurecido */}
          <div className="modal-backdrop fade show"></div>
        </>
      )}

      {/* Contenido */}
      <main className="dashboard-main container py-3">
        {children}
      </main>
    </div>
  );
}

export default DashboardLayout;