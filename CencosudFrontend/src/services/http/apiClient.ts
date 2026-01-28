// src/services/apiClient.ts
import axios from 'axios';
import {
  clearAuthSession,
  getAuthToken,
} from '../storage/localStorage.service';

const apiClient = axios.create({
  baseURL: 'https://localhost:7120/api',
});

apiClient.interceptors.request.use(
  (config) => {
    const token = getAuthToken();
    if (token) {
      config.headers = config.headers ?? {};
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// 🔐 Interceptor para manejar errores 401 (token vencido / inválido)
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status;

    if (status === 401) {
      // Limpia la sesión
      clearAuthSession();

      // Lanza un evento global que React escuchará
      window.dispatchEvent(new CustomEvent('session-expired'));
    }

    return Promise.reject(error);
  }
);

export default apiClient;
