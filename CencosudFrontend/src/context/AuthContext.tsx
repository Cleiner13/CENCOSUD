// src/context/AuthContext.tsx
import {
  createContext,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import type { LoginResponse } from '../types/auth';
import {
  clearAuthSession,
  getAuthSession,
  isAuthenticated as isAuthenticatedFromStorage,
  saveAuthSession,
  type AuthSession,
} from '../services/storage/localStorage.service';

interface AuthContextValue {
  user: AuthSession | null;
  isAuthenticated: boolean;

  esAsesor: boolean;
  esSupervisor: boolean;
  esAdmin: boolean;

  login: (data: LoginResponse) => void;
  logout: () => void;
  hasRole: (role: string) => boolean;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function computeRolesFromSession(session: AuthSession | null) {
  if (!session) {
    return {
      esAsesor: false,
      esSupervisor: false,
      esAdmin: false,
    };
  }

  const cargo = (session.cargo || '').toUpperCase();
  const rolAppRaw = (session.rolApp || '').toUpperCase();

  let esAsesor = rolAppRaw === 'ASESOR';
  let esSupervisor = rolAppRaw === 'SUPERVISOR';
  let esAdmin = rolAppRaw === 'ADMIN';

  if (!rolAppRaw) {
    esAsesor = cargo.includes('PROMOTOR');
    esSupervisor = cargo.includes('SUPERVISOR');
    esAdmin = !esAsesor && !esSupervisor;
  }

  return { esAsesor, esSupervisor, esAdmin };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthSession | null>(() => getAuthSession());

  const { esAsesor, esSupervisor, esAdmin } = useMemo(
    () => computeRolesFromSession(user),
    [user]
  );

  const value = useMemo<AuthContextValue>(() => {
    const isAuthenticated = isAuthenticatedFromStorage();

    const login = (data: LoginResponse) => {
      if (!data.token) return;
      saveAuthSession(data);
      setUser(getAuthSession());
    };

    const logout = () => {
      clearAuthSession();
      setUser(null);
    };

    const hasRole = (role: string) => {
      const roleUpper = role.toUpperCase();
      const rolApp = user?.rolApp?.toUpperCase();
      if (rolApp) return rolApp === roleUpper;
      // fallback por cargo
      if (roleUpper === 'ASESOR') return esAsesor;
      if (roleUpper === 'SUPERVISOR') return esSupervisor;
      if (roleUpper === 'ADMIN') return esAdmin;
      return false;
    };

    return {
      user,
      isAuthenticated,
      esAsesor,
      esSupervisor,
      esAdmin,
      login,
      logout,
      hasRole,
    };
  }, [user, esAsesor, esSupervisor, esAdmin]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuthContext(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error('useAuthContext debe usarse dentro de un AuthProvider');
  }
  return ctx;
}

export { AuthContext };

