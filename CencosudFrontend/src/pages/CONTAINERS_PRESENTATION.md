## Convenciones de contenedor vs presentación

- **Páginas contenedor (`Page`)**
  - Viven en `src/pages/**/**Page.tsx`.
  - Se encargan de:
    - Leer contexto global (auth, UI, etc.).
    - Orquestar hooks de datos (`useDashboardPage`, `useAdminReportesPage`, etc.).
    - Pasar `props` a componentes de presentación.
  - No deben contener estilos “a mano” ni JSX muy anidado; delegan en componentes de `src/components`.

- **Hooks de página (`useXxxPage`)**
  - Viven en `src/hooks/` (ej. `src/hooks/useDashboardPage.ts`).
  - Encapsulan:
    - Estado local de la vista.
    - Efectos (`useEffect`) y llamadas a servicios (`services/**`).
    - Transformaciones y `useMemo` para datos de gráficos/tablas.
  - Devuelven un objeto tipado con todos los datos y handlers que necesitan los componentes de presentación.

- **Componentes de presentación**
  - Viven en `src/components/**`.
  - Son *puros*: reciben `props` y renderizan JSX (sin llamadas a servicios ni `localStorage`).
  - Usan TailwindCSS para estilos (sin Bootstrap nuevo) y los componentes base (`Button`, `Card`, `PageHeader`, `FilterBar`, `ChartCard`).

- **Ejemplo recomendado**
  - `DashboardPage.tsx`:
    - Llama a `const vm = useDashboardPage();`
    - Renderiza la vista usando componentes como `PageHeader`, `ChartCard`, `Card`, etc.
  - `AdminReportesPage.tsx`:
    - Llama a `const vm = useAdminReportesPage();`
    - Usa `FilterBar`, `ChartCard` y tablas de presentación.

