import type { LoginResponse } from '../../types/auth';
import apiClient from '../http/apiClient';

export async function loginApi(data: { usuario: string; password: string }): Promise<LoginResponse> {
  const resp = await apiClient.post<LoginResponse>('/auth/login', data);
  return resp.data;
}
