# DailyNotes UI

React 19 + TypeScript + Vite frontend for the DailyNotes productivity platform.

## Development

```powershell
# From project root
npm install
npm run dev        # http://localhost:5173
npm run build
npm run preview
```

The dev server proxies `/api` requests to the .NET API at `http://localhost:5010` (configured in `vite.config.ts`).

## Stack

| Layer | Library |
|---|---|
| Framework | React 19 + TypeScript |
| Build | Vite 7 |
| Routing | React Router 7 |
| Server state | TanStack Query 5 |
| Client state | Zustand 5 |
| Rich text | Lexical 0.41 |
| HTTP client | Axios |
| Styling | Tailwind CSS 4 |
| Icons | Lucide React |

## Project structure

```
src/
  components/     Shared UI components
  pages/          Route-level page components
  stores/         Zustand stores (auth, UI preferences)
  lib/            API client, utilities, hooks
  assets/         Static assets
```

## API client

The API client in `src/lib/api.ts` uses Axios with a request interceptor that attaches the JWT Bearer token from local storage. Token refresh is handled automatically on 401 responses via the `/api/auth/refresh` endpoint (the refresh token is stored as an httpOnly cookie).

## Authentication flow

1. `POST /api/auth/login` → receives `{ token, expiration, tenantId, role }` + sets `refreshToken` httpOnly cookie
2. Token stored in Zustand store + local storage
3. On app load, if token is expired, `POST /api/auth/refresh` exchanges the cookie for a new token pair
4. `POST /api/auth/logout` clears the cookie

## ESLint

To enable stricter type-checked rules, update `eslint.config.js`:

```js
import tseslint from 'typescript-eslint'

export default tseslint.config({
  extends: [tseslint.configs.recommendedTypeChecked],
  languageOptions: {
    parserOptions: {
      project: ['./tsconfig.node.json', './tsconfig.app.json'],
      tsconfigRootDir: import.meta.dirname,
    },
  },
})
```
