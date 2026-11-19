# Web Portal Development

The web management portal is a React 19, TypeScript, Vite, and TanStack Query application.

## Install and run

```powershell
cd src/SportsTeamManagement.Web
npm install
copy .env.example .env.local
npm run dev
```

Set `VITE_API_BASE_URL` to the API HTTPS origin.

## Authentication

The portal accepts an externally issued bearer access token, stores it in browser local storage, and sends it to the API. This is suitable for development and integration; production browser deployments should use the issuer's OIDC authorization flow instead of manual token entry.

The active club is stored separately and sent through `X-Club-Id`.

## Validation

```powershell
npm run lint
npm run build
```

## Implemented screens

- Dashboard backed by clubs and attendance statistics
- Students filtered by status
- Training groups with active-club filtering and creation
- Bulk attendance entry
- Club configuration and club creation
