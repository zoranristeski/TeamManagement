# Authorization Matrix

## Roles

Initial application roles are PlatformAdmin, ClubAdmin, Trainer, Parent, Student, and optionally OfficeStaff.

JWT roles may be supplied by an external issuer and must map to application policies. Club membership supplies tenant and club-local authorization.

## Policies

| Capability | PlatformAdmin | ClubAdmin | Trainer | Parent | Student |
|---|---:|---:|---:|---:|---:|
| Manage clubs | Yes | Own/assigned club only | No | No | No |
| Manage club users | Yes | Yes | No | No | No |
| Manage students | Yes | Yes | Limited assigned students | Own children read-only | Own profile |
| Manage guardians | Yes | Yes | Restricted read | Own profile/children | No |
| Manage groups/trainers | Yes | Yes | Read assigned; limited operations | Read enrolled children | Read own groups |
| Create/reschedule sessions | Yes | Yes | Assigned sessions only | No | No |
| Record attendance | Yes | Yes | Assigned sessions only | No | No |
| View attendance | Yes | Yes | Assigned scope | Own children | Own |
| Manage memberships/payments | Yes | Yes | No | Own/child read; no writes | Own read |
| Manage belt promotions | Yes | Yes | Authorized grading scope | Own child read | Own read |
| Manage events | Yes | Yes | Assigned/authorized | Read/register own children | Read/register own |
| Reports | Platform/all authorized clubs | Club | Assigned scope | Own children | Own |

## Tenant and resource rules

- Every club request requires an authenticated user and an active club context that is a verified club membership.
- PlatformAdmin access across clubs must be explicit and audited.
- Parent access is permitted only through an authorized GuardianRelationship.
- Medical and emergency fields require additional sensitive-data policies.
- Trainer access is restricted to assigned groups and sessions unless ClubAdmin grants broader permissions.
- IDs are not sufficient: services must verify the club and resource relationship on every operation.
- UI visibility never replaces server authorization.
