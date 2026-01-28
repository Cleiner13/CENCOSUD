// src/services/ocrService.ts
import api from '../http/apiClient';
import type { OcrCencosudResponse, OcrCencosudCampos } from '../../types/ocr';

export async function leerImagenCencosudIA(file: File): Promise<OcrCencosudCampos> {
  const formData = new FormData();
  formData.append('Imagen', file); // 👈 mismo nombre que en tu DTO [FromForm] OcrCencosudResumenRequestDto

  const resp = await api.post<OcrCencosudResponse>(
    '/ocr/cencosud-resumen-ia',   // 👈 ruta de tu nuevo endpoint
    formData,
    {
      headers: { 'Content-Type': 'multipart/form-data' },
    }
  );

  return resp.data.campos;
}
