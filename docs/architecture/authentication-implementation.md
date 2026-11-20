# Authentication and User Management Implementation

## Implemented authentication

The API now provides centralized local and external authentication endpoints:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `GET /api/auth/me`
- `POST /api/auth/external/google`
- `POST /api/auth/external/facebook`
- `GET /api/auth/providers`
- `POST /api/auth/link-provider/{provider}`
- `DELETE /api/auth/unlink-provider/{provider}`
- Enrollment and club user-management endpoints under `/api/user-management`

Local passwords use PBKDF2-SHA256 with a random salt and a 100,000-iteration minimum.
Accounts lock after five failed attempts for 15 minutes. Access tokens are short-lived
and contain identity plus a token identifier. Refresh tokens are random 256-bit values
stored only as SHA-256 hashes. Refresh rotates tokens and revokes a token family on reuse.

## External providers

Google and Facebook authorization-code exchanges are performed by the backend. Provider
configuration must be supplied through environment variables or secrets:

- `Authentication__Google__ClientId`
- `Authentication__Google__ClientSecret`
- `Authentication__Facebook__AppId`
- `Authentication__Facebook__AppSecret`

The backend verifies returned provider identities. External-first registration creates
an application identity but never silently links a provider solely by matching email.
Provider linking requires an authenticated application account. A provider can be
unlinked only while another password or external authentication method remains.

For local JWT issuance, configure:

- `Authentication__SigningKey` (at least 32 bytes)
- `Authentication__LocalIssuer`
- `Authentication__Audience`

Never commit provider secrets or signing keys. Use environment variables or a secrets
manager. An external HTTPS issuer remains supported when no local signing key is configured.

## Club authorization

Platform administrators use platform-wide policies. Club operations use club-local
Trainer, ClubAdmin, Parent, and Student policies. Every club operation must validate
the active club membership and club ID. Frontend route guards are not authorization
boundaries.

Enrollment requests create no club membership until authorized approval. Student and
Parent approvals activate the requested membership. Role assignments are recorded with
actor, previous role, new role, reason, and audit history.

## Database migration

The additive migration `20261120215647_AddCentralizedIdentityAndEnrollment` adds:

- Local identity fields and normalized email
- External provider identities
- Hashed refresh tokens
- Enrollment requests
- Parent-student relationships
- Role assignment history
- Audit logs
- Club membership approval and status fields

Existing `Users`, `ClubMemberships`, and clubs are preserved. Existing normalized emails
and membership statuses are backfilled in the migration. Roll back with the EF migration
rollback command only after a database backup and application compatibility review.

## Client usage

React uses the same `/api/auth` API, provider callbacks, refresh-token rotation, and
secure-storage API client conventions. MAUI uses SecureStorage for access and refresh
tokens and can login, refresh, and sign out through the shared API.

## Remaining provider and account operations

Email verification, forgot/reset-password, provider email templates, and production
email delivery require an email provider and token entity. External OAuth provider
redirect URIs and secrets remain developer/configuration dependent. React and MAUI
provider flows require the corresponding registered OAuth client IDs and redirect URIs.
