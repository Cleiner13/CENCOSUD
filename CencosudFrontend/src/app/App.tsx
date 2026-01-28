// src/app/App.tsx
import type { ReactElement } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';

import LoginPage from '../pages/Login/LoginPage';
import DashboardPage from '../pages/Dashboard/DashboardPage';
import RegistroCencosudPage from '../pages/RegistroCencosud/RegistroCencosudPage';
import ReportesCencosudPage from '../pages/ReportesCencosud/ReportesCencosudPage';
import DashboardLayout from '../layouts/DashboardLayout/DashboardLayout';
import MetasPage from '../pages/Metas/MetasPage';
import AdminReportesPage from '../pages/AdminReportes/AdminReportesPage';
import { useAuth } from '../hooks/useAuth';

// 🔐 Protege las rutas que requieren login
type ProtectedRouteProps = {
  children: ReactElement;
};

function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return children;
}

function App() {
  return (
    <Routes>
      {/* Login */}
      <Route path="/login" element={<LoginPage />} />

      {/* Dashboard principal */}
      <Route
        path="/dashboard"
        element={
          <ProtectedRoute>
            <DashboardLayout>
              <DashboardPage />
            </DashboardLayout>
          </ProtectedRoute>
        }
      />

      {/* Registro Cencosud usando el mismo layout */}
      <Route
        path="/registro"
        element={
          <ProtectedRoute>
            <DashboardLayout>
              <RegistroCencosudPage />
            </DashboardLayout>
          </ProtectedRoute>
        }
      />

     {/* Reportes de ventas */}
      <Route
        path="/reportes"
        element={
          <ProtectedRoute>
            <DashboardLayout>
              <ReportesCencosudPage />
            </DashboardLayout>
          </ProtectedRoute>
        }
      />
      
      {/* Metas */}
      <Route
        path="/metas"
        element={
          <ProtectedRoute>
            <DashboardLayout>
              <MetasPage />
            </DashboardLayout>
          </ProtectedRoute>
        }
      />

      {/* Reportes de ventas */}
      <Route
        path="/admin-reportes"
        element={
          <ProtectedRoute>
            <DashboardLayout>
              <AdminReportesPage  />
            </DashboardLayout>
          </ProtectedRoute>
        }
      />

      {/* Ruta por defecto */}
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}

export default App;
