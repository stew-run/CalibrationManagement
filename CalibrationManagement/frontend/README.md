# Calibration Management – Frontend

React 18 + Vite 5 + React Router 6 + Axios + Framer Motion (plain JavaScript / JSX). Talks to the ASP.NET Core API only through `src/api`.

```powershell
npm install                        # also creates package-lock.json - keep it
npm run dev                        # http://localhost:5173
npm run build                      # production build into dist/ (reads .env.production)
```

`.env` (development) and `.env.production` set `VITE_API_BASE_URL`; it must end with `/api`. Edit `.env.production` before building for deployment.

```
src/
├── api/          axiosClient.js (base URL, attaches the JWT, turns 401 into a sign-out, ApiError), http.js, one *Api.js per resource, index.js barrel
├── auth/         AuthContext.jsx, useAuth.js, ProtectedRoute.jsx (must be signed in), RoleGuard.jsx (needs a permission), tokenStorage.js
├── features/     equipment, schedules, results, certificates, maintenance, notifications, history, reports, users (pages/)
├── components/   shared UI: ui/ (Button, Modal, DataTable, ...) and ResourcePage.jsx
├── layouts/      MainLayout.jsx (+ Sidebar, Topbar), AuthLayout.jsx
├── pages/        LoginPage, DashboardPage
├── routes/       AppRoutes.jsx
├── contexts/     Toast, Theme, Lookups providers
├── hooks/        useApi, useListQuery, useDebounce, useMediaQuery
├── types/        JSDoc API contracts
├── utils/        format.js, motion.js (shared animation variants)
├── App.jsx  main.jsx  index.css
```

Notes: the JWT is kept in `sessionStorage` (cleared when the tab closes); there is no refresh-token flow because the API issues a single short-lived access token, so a 401 signs the user out. The theme is applied by an inline script in `index.html` before first paint; all animations honour `prefers-reduced-motion`.
