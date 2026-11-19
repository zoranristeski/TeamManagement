# API Contract Strategy

## Existing routes

The API currently exposes:

- `api/users`
- `api/students`
- `api/attendance`

Controllers inherit an API route convention that should remain under `api/v{version}` for new contracts; existing routes must be preserved during migration or documented as legacy aliases.

## Required route groups

- `/api/v1/clubs`
- `/api/v1/identity`
- `/api/v1/users`
- `/api/v1/guardians`
- `/api/v1/students`
- `/api/v1/trainers`
- `/api/v1/groups`
- `/api/v1/schedules`
- `/api/v1/sessions`
- `/api/v1/attendance`
- `/api/v1/memberships`
- `/api/v1/payments`
- `/api/v1/grading`
- `/api/v1/events`
- `/api/v1/reports`

## Conventions

- Use DTO request/response types; never expose entities or Infrastructure types.
- Use cancellation tokens and async repository/application operations.
- Use standard `ProblemDetails` responses and documented validation failures.
- Use `401` for unauthenticated requests, `403` for authorization failures, `404` for hidden or missing resources, `409` for conflicts, and `422` for domain validation where appropriate.
- Use cursor or offset pagination for lists. Offset pagination will initially use `page`, `pageSize`, `sort`, and filter fields. Return item count metadata.
- Bulk attendance must be atomic and idempotent.
- Schedule creation must detect conflicts and prevent duplicate generated occurrences.
- Mutating operations should include club context implicitly from validated active-club claims/context and explicitly audit actor and tenant.

## Authentication contracts

The API will not own passwords until Identity is deliberately implemented. The external issuer remains responsible for login and refresh. Required application endpoints:

- `GET /identity/me`
- `POST /identity/active-club`
- `POST /users/activate`
- `POST /users/deactivate`

Password reset and account lifecycle require ASP.NET Core Identity or an explicitly integrated identity provider. They must not be simulated in the database.
