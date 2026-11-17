# Security assessment and deployment checklist

**Classification: POC only; not production-ready for real student or child data.** The API is now fail-closed for authentication and default authorization, but important identity-provider and business-level access controls remain owner decisions.

## Implemented controls

- [x] API validates JWT bearer tokens against externally supplied HTTPS issuer and audience. Missing connection/auth configuration stops startup.
- [x] All controller endpoints require `Admin` by the fallback policy unless explicitly assigned a narrower authorization rule.
- [x] Student self-registration requires a valid token, and the API takes the stable identity from the validated JWT `sub` claim rather than trusting the request's `Auth0Id`.
- [x] Swagger/OpenAPI is only mapped in Development.
- [x] CORS permits only exact configured HTTPS origins; no origin is assumed. HTTP loopback is accepted only in Development. Empty allowlist denies browsers.
- [x] HTTPS redirection/HSTS and App Service HTTPS-only are part of the deployment procedure.
- [x] Global rate limiting is set to 120 requests per authenticated subject per minute; health probes are exempt.
- [x] Exceptions are logged server-side with a trace ID and returned as generic Problem Details. Health responses do not disclose SQL/server details.
- [x] Production uses JSON console logs; EF command logs remain at Warning. Request bodies/medical data are not logged by the API.
- [x] App Service-to-SQL uses a system-assigned managed identity and TLS, not a SQL password. The runtime SQL principal receives only reader/writer permissions.
- [x] EF Core parameterizes ordinary LINQ queries. No `FromSql`/`ExecuteSql` usage was found in the reviewed source. Continue parameterizing any future raw SQL.
- [x] SQL migrations are explicit and reviewed; API startup does not call `EnsureCreated` or automatically apply migrations.
- [x] High-severity AutoMapper 15.0.1 advisories reported by NuGet restore were addressed by upgrading to patched 15.1.4.

## Identity-provider setup required before deployment

- [ ] Supply the actual provider `Authentication:Authority` and API `Authentication:Audience`; these are absent from the source and must not be guessed.
- [ ] Configure the identity provider to issue signed API access tokens with the correct issuer/audience.
- [ ] Configure the role claim and set `Authentication:RoleClaimType` to its actual claim name. Default is `roles`.
- [ ] Provision trusted administrators through the identity provider. Never allow a public request to grant itself `Admin`.
- [ ] Confirm access token signature, issuer, audience, expiry, and role claims in a secure test. The local integration tests use a test-only authentication scheme; they do not prove the real provider configuration works.
- [ ] Decide whether Auth0 remains the provider. The existing `Auth0Id` field is only a clue, not proof of an existing configured tenant/client.

## Student, child, and multi-club data boundaries

The existing student DTO contains date of birth, emergency contact information, and free-text medical conditions. The current entity model has no demonstrated club/tenant ownership boundary, guardian/consent model, or student-to-authenticated-user relationship suitable for enforcing per-student access. Consequently:

- All list/detail/update/attendance/student controller APIs are Admin-only by default; trainers and students cannot access them until API-side policies and data ownership are implemented and tested.
- Registration is available to any valid authenticated token. It binds the new account's `Auth0Id` to that token's `sub`, but it does not verify that a person is legally authorized to register a minor, validate guardian consent, or restrict submitted medical/emergency fields.
- The data model does not provide verified multi-club tenant isolation. Do not expose records to multiple clubs or grant trainer/student roles until tenant ownership and per-object authorization are implemented at the API/service/repository level.
- The current `StudentDto` returns sensitive fields. The Admin-only policy reduces exposure but is not a substitute for data minimization, purpose limitation, access auditing, consent, retention, and legal review.
- Restrict administrative accounts, require MFA at the identity provider, and audit privileged account assignment.

These gaps require business/model and identity decisions and are not solved by hiding frontend pages. Treat any use of actual child/student data as blocked until these controls are designed, implemented, tested, and reviewed.

## Data, database, and networking

- [ ] SQL uses Entra-only authentication and the App Service managed identity; firewall access is limited to the required operator and app outbound addresses. No global Azure-services firewall rule or open SQL endpoint is permitted.
- [ ] Runtime database user is not `db_owner` and cannot apply migrations. DDL is applied by a separate controlled operator.
- [ ] Confirm Azure SQL encryption at rest/TDE status and encrypted TLS transport; verify data residency and backup redundancy meet policy.
- [ ] Do not put SQL passwords, API keys, bearer tokens, connection strings with credentials, student records, or medical data into Git, GitHub logs, app logs, issue comments, screenshots, or chat.
- [ ] Limit App Service/GitHub/Azure SQL administrative roles; review access periodically.
- [ ] Confirm firewall allowlists after any App Service scale/move/recreate; F1 has public networking and does not provide this design a private endpoint.
- [ ] Set backup retention and export/recovery locations to meet the actual retention/data-residency obligation. Free POC backup limits and defaults are not a production recovery plan.

## Input validation, abuse, and monitoring

- [ ] `[ApiController]` provides framework model-state handling, but existing DTOs do not establish comprehensive business validation for date ranges, phone/email formats, enum values, required fields, or field length constraints. Add specific validation and business-rule tests before production.
- [ ] The rate limit is a simple per-subject fixed window, not a WAF, DDoS protection, or abuse-detection service. Tune it for realistic client behavior; F1 resource quotas can be exhausted by legitimate or malicious traffic.
- [ ] No user/role access audit trail or privacy/security alerting was found. Implement and verify one before real student data is used.
- [ ] Do not log request/response bodies or EF parameter values. Configure operational retention/monitoring and verify access controls on any future App Insights/Log Analytics workspace (both can incur cost).
- [ ] Perform security testing of the actual identity provider, deployed app, rate limit, CORS, SQL firewall, and API routes. The included integration tests cover only basic middleware behavior.

## Production go/no-go

**Do not classify as production-ready** until all unchecked identity, guardian/consent, tenant ownership, object authorization, input-validation, audit, backup/recovery, and operational monitoring items have owners and verified implementations. The current code-level role gate is an improvement for a POC, not proof of compliant access to child data.
