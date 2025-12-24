# Repository Guidelines

## Project Structure & Module Organization
OneAI is a split frontend/backend repo. `web/` is the React/Vite UI; key folders: `web/src/components/`, `web/src/pages/`, `web/src/services/`, `web/src/types/`, `web/src/router/`, `web/src/lib/`. `src/OneAI/` is the .NET 10 API; key folders: `Data/` (EF Core), `Entities/`, `Services/`, `Models/` (DTOs), `Endpoints/`, with `Program.cs` wiring endpoints. Runtime config lives in `src/OneAI/appsettings.json`; frontend config in `web/.env`. SQLite `oneai.db` is created on first run.

## Build, Test, and Development Commands
Frontend (from `web/`):
- `npm install` install deps
- `npm run dev` start Vite at `http://localhost:5173`
- `npm run build` typecheck + production build in `web/dist/`
- `npm run preview` serve build
- `npm run lint` run ESLint

Backend (from `src/OneAI/`):
- `dotnet run` start API at `http://localhost:5000`
- `dotnet publish -c Release -o publish` build release output in `src/OneAI/publish/`

## Coding Style & Naming Conventions
- C# uses 4-space indentation, nullable enabled, PascalCase for types/methods, and `I*` for interfaces (e.g., `IAuthService`). Endpoints are grouped as extension methods under `Endpoints/` (`Map*Endpoints`).
- React/TypeScript uses 2-space indentation, PascalCase component names (`Home.tsx`), camelCase for locals, and the `@/` alias for `web/src` (see `web/vite.config.ts`).
- Linting: `web/eslint.config.js` uses ESLint + typescript-eslint + react-hooks + react-refresh.

## Testing Guidelines
No automated test projects or scripts are configured yet, and there are no coverage targets. If you add tests, also add a script (e.g., `dotnet test` or `npm run test`) and document the location and naming convention in the relevant README.

## Commit & Pull Request Guidelines
Git history currently has a single initial commit with a short, non-English summary, so there is no established convention. Use concise, one-line summaries in the imperative voice. PRs should include: a short description, linked issues when applicable, testing notes, and screenshots/GIFs for UI changes.

## Security & Configuration Tips
Update `src/OneAI/appsettings.json` and `web/.env` for local settings; never commit real secrets. Change the JWT secret and default admin password before any production deployment, and review CORS origins in `src/OneAI/Program.cs` when exposing the API.
