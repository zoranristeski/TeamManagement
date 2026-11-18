# Backend Gap Analysis

## Existing implementation

The solution uses .NET 10 Clean Architecture projects for Domain, Application, Infrastructure, API, and MAUI. The API uses JWT bearer authentication, an HTTPS issuer requirement, CORS allowlisting, global rate limiting, health checks, and JSON logging outside Development.

Existing entities: User, Student, Trainer, TrainingSession, Attendance, BeltPromotion, Event, and EventRegistration.

Existing controllers: Users, Students, and Attendance. The repository also contains application services and repositories for users, students, attendance, and training sessions.

Existing tests cover anonymous access, the Admin fallback policy, role rejection, CORS, and health checks.

## Critical gaps

1. No Club or tenant entity, club membership, active-club context, or tenant query enforcement.
2. No login, refresh, logout, profile, password, activation, or identity provisioning implementation in the application. Authentication delegates to an external issuer.
3. The fallback policy requires only an authenticated Admin role, making Student, Parent, Trainer, and future endpoint policies unavailable or blocked by default.
4. No explicit authorization policies, role/permission matrix, resource ownership checks, or parent-child restrictions.
5. No guardian entity or child relationship.
6. No training group entity or student/group/trainer assignments.
7. TrainingSession has no group or location and does not model recurrence or exceptions.
8. No bulk attendance endpoint or duplicate prevention.
9. No membership plan, membership, or payment entities.
10. BeltPromotion exists, but promotion APIs, requirements, grading events, and eligibility workflows are absent.
11. Event registration DTO/domain support exists only partially, with no Event or registration controller.
12. No reports/dashboard data endpoint.
13. No pagination, sorting, filtering, Fluent-style validation, or standard result/problem response conventions.
14. API routes are not consistently versioned.
15. The MAUI project remains net9.0 template code and has no API, authentication, or MVVM implementation.
16. No web project exists.
17. Test coverage has no domain, application, repository, tenant, or end-to-end workflow tests.

## Implementation sequence

1. Add explicit authorization and active-club/tenant infrastructure.
2. Add Club, club membership, Guardian, TrainingGroup, membership, and payment foundations.
3. Complete user, student, trainer, group, schedule, attendance, grading, event, and report endpoints.
4. Add validation, pagination, filtering, problem responses, migrations, and integration tests.
5. Establish the React web foundation, design system, authentication, and API client.
6. Implement web modules against live endpoints.
7. Upgrade or separately retain the MAUI target strategy and implement authentication, Shell, services, and role views.
8. Add workflow tests, documentation, and CI/deployment artifacts.
