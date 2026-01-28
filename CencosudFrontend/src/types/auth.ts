export interface LoginRequest {
  usuario: string;
  password: string;
}

export interface LoginResponse {
  codigoResultado: number;
  mensaje: string;
  token?: string;
  idUsuario?: number;
  usuario?: string;
  estado?: string;
  cargo?: string;
  supervisor?: string;
  uunn?: string;
  rolApp?: 'ASESOR' | 'SUPERVISOR' | 'ADMIN' | string;
}
