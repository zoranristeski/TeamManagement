# Client Application Architecture

## Web portal

Create `src/SportsTeamManagement.Web` as a React, TypeScript, and Vite application. Use React Router for navigation, TanStack Query for API state, React Hook Form plus Zod for validated forms, and a small generated or hand-maintained typed API client until contract generation is configured.

The web application will use:

- A permission-aware application shell with sidebar, top bar, club switcher, breadcrumbs, and responsive navigation.
- Feature folders for dashboard, students, guardians, trainers, groups, calendar, attendance, memberships, grading, events, reports, and settings.
- Shared design-system components for fields, tables, dialogs, pagination, status badges, notifications, loading states, empty states, and errors.
- TanStack Query cache keys containing club and active user context.
- API authorization through JWT bearer tokens; no domain or infrastructure project references.

## Mobile application

Replace the default MAUI template in `src/SportsTeamManagement.Mobile` with an MVVM application using CommunityToolkit.Mvvm, dependency injection, Shell navigation, SecureStorage for tokens, and typed HttpClient services.

Recommended folders:

- `App`
- `Views`
- `ViewModels`
- `Models`
- `Services`
- `Api`
- `Controls`
- `Converters`
- `Resources`

Mobile navigation will be role-aware and prioritize today's sessions and trainer attendance. The first implementation will be online-first. Offline attendance can be added later with explicit pending-operation, conflict, and synchronization semantics.

## Shared integration

Both clients consume the REST API. OpenAPI remains the contract source. Web and mobile response models must stay field-compatible with API DTOs. Authentication, authorization, tenant isolation, validation, and scheduling rules remain in the backend.
