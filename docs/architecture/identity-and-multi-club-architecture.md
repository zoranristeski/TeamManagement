## Centralized Identity and Multi-Club Architecture

### Repository baseline

The solution is a .NET 10 Clean Architecture application with API, Application, Domain,
Infrastructure, React/Vite, and MAUI projects. The API currently validates JWT bearer
tokens issued by an external HTTPS authority and applies a single authenticated fallback
policy. It does not yet provision local identities, issue application JWTs, rotate refresh
tokens, or implement ASP.NET Core Identity account flows.

Existing entities include `User`, `Student`, `Trainer`, `Club`, `ClubMembership`,
`TrainingGroup`, `Guardian`, membership plans, memberships, and payment records. The
existing `User.Role` field and role-like values are retained for compatibility, but club-
scoped capabilities must come from active `ClubMembership` records. A global identity
must remain stable across all club memberships.

The React application has a centralized token context and protected routes, but currently
stores only an access token in local storage and does not implement registration,
password recovery, provider login, refresh, or server-backed account linking. The MAUI
application has a secure-storage API client but is not a complete authentication client.

### Target architecture

The backend will remain the single source of identity and authorization:

- The API exposes account, external-login, refresh, membership, and enrollment use cases.
- ASP.NET Core Identity or an equivalent strongly validated local identity implementation
  owns password hashing, lockout, verification, and password reset where added.
- `User` remains the single global identity; `Auth0Id` is compatibility data and must not
  become an independent identity model.
- `ExternalLogin` associates provider identities with the global `User` ID.
- `ClubMembership` is the only source of club-local role and active membership status.
- `RefreshToken` stores a SHA-256 or stronger hash, family identifier, replacement, expiry,
  and revocation state. Plaintext refresh tokens are never stored.
- `EnrollmentRequest`, `ParentStudentRelationship`, role history, and `AuditLog` preserve
  workflow and security history.

JWT access tokens contain identity and platform claims only. Memberships and permissions
are evaluated from the database for every club-scoped operation.

### Authorization model

Global roles are limited to platform administration and explicitly named platform
capabilities. Student, Parent, and Trainer are club membership roles and must not be
assigned as mutually exclusive global roles.

Every protected club operation resolves:

1. The authenticated global user ID.
2. The requested club context.
3. An active verified club membership.
4. The membership role and permission set.
5. Resource ownership or parent-child relationship, when applicable.

UI route restrictions are informational only. Authorization handlers and policies remain
the security boundary.

### Database migration strategy

New tables and indexes must be additive. Existing users, clubs, memberships, and EF
Core history remain intact. Email normalization and external-provider uniqueness need
unique indexes. Password and refresh-token fields must never be persisted in plaintext.
The initial migrations introduce domain structures first, then authentication data and
flows. Existing external issuer subjects should be migrated without creating duplicate
application users.

### API and client changes

The API will add `/api/auth/*`, enrollment, club membership, parent relationship, role
management, audit, and notification capabilities. Existing controllers and DTO contracts
remain where compatible.

React and MAUI consume the same endpoints and provider flows. React should use an
in-memory access token with HttpOnly refresh/session cookies where deployment supports
it, plus CSRF protection. MAUI must use platform secure storage. Provider secrets remain
only in backend configuration.

### Security decisions

- Public registration cannot assign global platform roles or active club memberships.
- External identities are not linked solely by matching email addresses.
- Refresh tokens rotate and reuse detection revokes the token family.
- Provider authorization codes are validated by the backend; client-supplied profile
  claims are never trusted.
- Enrollment and role operations require club authorization and are audit logged.
- Login and registration responses avoid account-enumeration details.
- Cross-club resource access is denied even when a global JWT role would normally pass.

### Implementation phases

1. Repository and architecture analysis.
2. Identity and domain infrastructure plus migrations.
3. Email/password authentication and token lifecycle.
4. Google/Facebook external identity validation and account linking.
5. Centralized global and club-scoped authorization.
6. Enrollment and club user management.
7. React authentication and management.
8. MAUI authentication and management.
9. Automated, integration, and security testing.
10. Documentation, migration verification, builds, and release review.
