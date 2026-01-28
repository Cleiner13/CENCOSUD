# Estructura del proyecto (reordenada)

Esta versión organiza el **frontend** por responsabilidad (app / pages / layouts / services / types / styles),
para que otro programador pueda ubicar rápido cada parte.

## Carpetas principales

- `src/app/`
  - `App.tsx`: routing (React Router) + `ProtectedRoute`.
- `src/layouts/`
  - `DashboardLayout/`: layout compartido para las pantallas internas (topbar, logout, modal de sesión).
- `src/pages/`
  - Cada page vive en su propia carpeta, con su **TSX + CSS**:
    - `Login/`
    - `Dashboard/`
    - `RegistroCencosud/`
    - `ReportesCencosud/`
- `src/services/`
  - `http/`: cliente HTTP compartido (`apiClient.ts` con interceptores).
  - `auth/`: llamadas de autenticación.
  - `cencosud/`: llamadas del módulo Cencosud (tienda + reportes).
  - `ocr/`: llamadas OCR/IA.
- `src/types/`
  - Tipos/dtos centralizados por dominio.
- `src/styles/`
  - `globals.css`: reset + estilos base globales (solo lo que aplica a toda la app).

## Regla de estilos (CSS)

- **Global** (`src/styles/globals.css`): solo reset, body, #root y ajustes generales (ej. `.card`).
- **Por page**: cada pantalla importa su propio CSS:
  - `src/pages/*/*.tsx` importa `./<Page>.css`
- **Por layout**: el layout importa su propio CSS:
  - `src/layouts/DashboardLayout/DashboardLayout.tsx` importa `./DashboardLayout.css`

## Arranque

- `src/main.tsx` importa:
  - Bootstrap
  - `src/styles/globals.css`
  - `src/app/App.tsx`
