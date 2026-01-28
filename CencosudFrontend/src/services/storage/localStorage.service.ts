// src/services/storage/localStorage.service.ts
import type { LoginResponse } from '../../types/auth';

const TOKEN_KEY = 'token';
const USUARIO_KEY = 'usuario';
const CARGO_KEY = 'cargo';
const SUPERVISOR_KEY = 'supervisor';
const UUNN_KEY = 'uunn';
const ROL_APP_KEY = 'rolApp';
const ID_USUARIO_KEY = 'idUsuario';
const ESTADO_KEY = 'estado';

export interface AuthSession {
  token: string;
  usuario: string;
  cargo: string;
  supervisor: string;
  uunn: string;
  rolApp?: string;
  idUsuario?: number;
  estado?: string;
}

export function saveAuthSession(resp: LoginResponse): void {
  if (!resp.token) return;

  localStorage.setItem(TOKEN_KEY, resp.token);
  localStorage.setItem(USUARIO_KEY, resp.usuario ?? '');
  localStorage.setItem(CARGO_KEY, resp.cargo ?? '');
  localStorage.setItem(SUPERVISOR_KEY, resp.supervisor ?? '');
  localStorage.setItem(UUNN_KEY, resp.uunn ?? '');

  if (resp.rolApp) {
    localStorage.setItem(ROL_APP_KEY, resp.rolApp);
  }

  if (resp.idUsuario != null) {
    localStorage.setItem(ID_USUARIO_KEY, String(resp.idUsuario));
  }

  if (resp.estado != null) {
    localStorage.setItem(ESTADO_KEY, resp.estado);
  }
}

export function getAuthSession(): AuthSession | null {
  const token = localStorage.getItem(TOKEN_KEY);
  if (!token) return null;

  const usuario = localStorage.getItem(USUARIO_KEY) ?? '';
  const cargo = localStorage.getItem(CARGO_KEY) ?? '';
  const supervisor = localStorage.getItem(SUPERVISOR_KEY) ?? '';
  const uunn = localStorage.getItem(UUNN_KEY) ?? '';
  const rolApp = localStorage.getItem(ROL_APP_KEY) ?? undefined;

  const idUsuarioRaw = localStorage.getItem(ID_USUARIO_KEY);
  const idUsuario =
    idUsuarioRaw != null && idUsuarioRaw !== '' ? Number(idUsuarioRaw) : undefined;

  const estado = localStorage.getItem(ESTADO_KEY) ?? undefined;

  return {
    token,
    usuario,
    cargo,
    supervisor,
    uunn,
    rolApp,
    idUsuario,
    estado,
  };
}

export function clearAuthSession(): void {
  [
    TOKEN_KEY,
    USUARIO_KEY,
    CARGO_KEY,
    SUPERVISOR_KEY,
    UUNN_KEY,
    ROL_APP_KEY,
    ID_USUARIO_KEY,
    ESTADO_KEY,
  ].forEach((key) => localStorage.removeItem(key));
}

export function getAuthToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function isAuthenticated(): boolean {
  return !!getAuthToken();
}

