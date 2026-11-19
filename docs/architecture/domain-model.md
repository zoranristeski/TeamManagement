# Domain Model

## Existing entities

- `User`: identity subject, profile, role, activation state, optional student/trainer profile.
- `Student`: date of birth, emergency and medical information, join status, belt state, and related attendance/promotions/events.
- `Trainer`: trainer profile and qualifications (inspect implementation before extension).
- `TrainingSession`: name, description, time range, trainer, capacity, active state, and attendance.
- `Attendance`: student/session status and recorded information.
- `BeltPromotion`: historical belt promotion.
- `Event`: scheduled event, capacity, cost, deadline, organizer, and registrations.
- `EventRegistration`: student registration and confirmation state.

## Required additions

### Tenant foundation

- `Club`: identity, name, logo, location, configuration, active state.
- `ClubMembership`: club, user, role, active state, and platform-level distinction.
- `ClubConfiguration`: enabled disciplines, belt module, branding, and sport-specific settings.
- `SportDiscipline`: configurable discipline used by clubs and groups.

### People and relationships

- `Guardian`: or `GuardianProfile` associated with a User.
- `GuardianRelationship`: guardian, student, relationship type, authorization scope, and emergency/medical visibility authorization.

### Training

- `TrainingGroup`: club, name, description, discipline, age range, skill level, capacity, location, status, trainers, students, and schedule.
- `TrainingGroupStudent`: membership dates and active state.
- `TrainingGroupTrainer`: assignment and role.
- `TrainingSchedule`: recurring weekly rule and location.
- `ScheduleException`: cancellation, reschedule, or override for a specific occurrence.

### Commerce

- `MembershipPlan`: club, type, price, billing interval, active state.
- `Membership`: student, plan, period, status.
- `PaymentRecord`: membership/student payment amount and status. Payment processing remains external.

### Grading

- `PromotionRequirement`: discipline/rank requirement.
- `GradingEvent`: club, schedule, participants, status.
- `BeltPromotion`: extend with club, discipline, grading event, promoter, and notes.

## Tenant ownership

Every club-owned entity must contain `ClubId` or belong through a required club-owned aggregate. The active club must be validated against a user club membership before reads or writes. Queries must apply both club and user/resource authorization.
