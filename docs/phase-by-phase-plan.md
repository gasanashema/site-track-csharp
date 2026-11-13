# Phase-by-Phase Plan: SiteTrack Construction Manager (.NET WinForms MVC)

Source: plan.md

## Progress
- [x] Phase 1: Solution Architecture, Database Schema & Entity Models
- [x] Phase 2: Data Access Layer, Connection Factory & Seed Migration
- [x] Phase 3: User Authentication & Role-Based Main Shell
- [x] Phase 4: User Management & Project Administration with Site Scoping
- [ ] Phase 5: Material Catalog, Purchases & Weighted Average Stock Tracking
- [ ] Phase 6: Material Consumption (Usage Guard) & Immutable Audit Trail
- [ ] Phase 7: Worker Registration, Daily Attendance & Attendance-Gated Payments
- [ ] Phase 8: Site Activity Tracking & Progress Monitoring
- [ ] Phase 9: Reports & Data Export (PDF, Excel, CSV)
- [ ] Phase 10: End-to-End Verification, Automated Tests & Release Polish

## Phases

### Phase 1: Solution Architecture, Database Schema & Entity Models
**Goal:** Establish the .NET solution structure, define all 16 database table DDL schemas with relational constraints, and create strongly-typed domain entity classes following MVC standards.
**Tasks:**
- Create the Visual Studio solution (`SiteTrack.sln`) with layered projects (`SiteTrack.Core`, `SiteTrack.Data`, `SiteTrack.Controllers`, `SiteTrack.UI`, `SiteTrack.Tests`).
- Write complete SQL DDL schema script defining all 16 tables, primary keys, foreign keys, unique indexes, and check constraints.
- Implement the 16 C# POCO entity classes in `SiteTrack.Core.Models` with proper property types, nullability, and relationships.
- Implement custom validation exception classes and response result wrappers (`Result<T>`).
**Acceptance criteria:**
- The solution compiles cleanly with target framework .NET Framework 4.8 / .NET standard.
- SQL DDL script executes without errors on the target database, creating all 16 tables with correct foreign keys.
- All 16 entity classes exist with properties matching database columns exactly.
**Changelog:**
- 2026-11-13: Phase 1 approved and marked complete. Visual Studio solution structure, 16-table schema scripts, POCO models, enums, Result wrappers, and test suite verified with 100% pass rate.

---

### Phase 2: Data Access Layer, Connection Factory & Seed Migration
**Goal:** Implement robust parameterized ADO.NET database connection management, database initialization/migration runners, and base repository abstractions.
**Tasks:**
- Create `IDbConnectionFactory` and `DbConnectionFactory` supporting configurable connection strings (PostgreSQL / SQLite).
- Create `DbHelper` utility for executing parameterized queries, scalar queries, and safe transactional operations.
- Implement database initializer that automatically executes table creation if tables do not exist.
- Implement `002_seed_data.sql` script seeding default Administrator account, sample project, material categories, and worker types.
- Build repository interfaces (`IUserRepository`, `IProjectRepository`, `IMaterialRepository`, `IStockRepository`, `ILaborRepository`).
**Acceptance criteria:**
- Database connection test succeeds upon startup.
- Initial seed data creates an active Admin user and default reference catalogs.
- Repositories run parameterized SQL queries preventing any SQL injection vulnerability.
**Changelog:**
- 2026-11-13: Phase 2 approved and marked complete. Implemented `DbConnectionFactory`, `SiteTrackDataStore`, `DbHelper`, 6 concrete repositories, and 15 automated integration tests.

---

### Phase 3: User Authentication & Role-Based Main Shell
**Goal:** Deliver secure direct user authentication (username and password) and the master navigation shell with role-based UI restriction (OTP challenge omitted per user directive).
**Tasks:**
- Implement secure password hashing and verification (PBKDF2/SHA-256).
- Implement `AuthController` with login validation, active status enforcement, and session context creation.
- Build `FormLogin` with username/password inputs, keyboard shortcut navigation, and ErrorProvider validation.
- Build `FormMainShell` featuring a modern responsive sidebar, status bar showing logged-in user and role, logout flow, and conditional navigation visibility (Admin vs. Site Manager).
**Acceptance criteria:**
- Users cannot log in with invalid credentials or if their account is inactive.
- Valid credentials authenticate immediately and open `FormMainShell` without any OTP challenge.
- Admin sees all navigation modules; Site Manager sees only operational modules for assigned projects.
**Changelog:**
- 2026-11-13: Phase 3 approved and marked complete. Implemented `PasswordHasher`, `AuthController`, `FormLogin`, `FormMainShell` with role-based UI restriction, and 22 passing automated tests.

---

### Phase 4: User Management & Project Administration with Site Scoping
**Goal:** Implement user account administration for Admins and multi-project management with Site Manager assignments and scoping guards.
**Tasks:**
- Implement `UserController` and `UcUsers` user control for creating, editing, and toggling user active status.
- Add technical validation ensuring username and email uniqueness across users.
- Implement `ProjectController` and `UcProjects` for project lifecycle management (Planning, Ongoing, Completed, Cancelled).
- Build Site Manager assignment dialog mapping managers to projects via `project_managers`.
- Implement project scoping logic: Site Managers are strictly constrained to projects assigned to them.
- Implement deletion protection preventing removal of projects with active stock or labor records.
**Acceptance criteria:**
- Admin can create and deactivate users and assign managers to projects.
- Duplicate usernames or emails are rejected with descriptive error notifications.
- Site Manager dropdowns only display projects assigned to that manager in `project_managers`.
- Attempting to delete a project with child transactions is blocked with a validation warning.
**Changelog:**
- 2026-11-13: Phase 4 approved and marked complete. Implemented `UserController`, `ProjectController`, `UcUsers`, `UcProjects`, `FormUserDialog`, `FormProjectDialog`, `FormAssignManagerDialog`, and 30 automated unit/integration tests.

---

### Phase 5: Material Catalog, Purchases & Weighted Average Stock Tracking
**Goal:** Implement material master catalogs, material purchase recording, and real-time weighted average stock calculation.
**Tasks:**
- Build `UcMaterials` for managing material categories and individual material items with units and reference prices.
- Build material purchase recording dialog (`material_purchases`) with supplier, quantity, and unit price inputs.
- Implement stock transaction handler: when a purchase is saved, atomically update `project_material_stock` balance and recalculate weighted average unit price.
- Insert an `IN` record into `material_stock_movements` referencing the purchase.
- Build `UcStock` displaying real-time stock balances per project with visual highlight for materials below `minimum_quantity`.
**Acceptance criteria:**
- Adding a purchase updates `quantity_available` in `project_material_stock`.
- The weighted average unit price formula recalculates correctly: `((old_qty * old_avg) + (new_qty * new_price)) / (old_qty + new_qty)`.
- An immutable `IN` movement entry is recorded in `material_stock_movements`.
- Materials with stock below `minimum_quantity` are visually highlighted in the grid.
**Changelog:**
- 2026-11-13: Phase 5 approved and marked complete. Implemented `MaterialController`, `StockController`, `UcMaterials`, `UcStock`, `FormCategoryDialog`, `FormMaterialDialog`, `FormPurchaseDialog`, weighted average price recalculation, and 8 automated tests (total 38 tests passing).

---

### Phase 6: Material Consumption (Usage Guard) & Immutable Audit Trail
**Goal:** Implement daily material usage recording with stock depletion prevention, stock adjustment for physical counts, and full audit trail exploration.
**Tasks:**
- Build `UcMaterialUsage` for Site Managers to log daily material usage with activity description.
- Implement Business Rule 1: strictly reject usage if `quantity_used > quantity_available`.
- Implement usage transaction: atomically decrement `quantity_available`, insert `material_usage`, and insert an `OUT` movement record.
- Implement stock adjustment dialog for Admins to record physical count corrections (`ADJUSTMENT` movement).
- Build stock movement audit trail view in `UcStock` displaying full historical movements (IN, OUT, ADJUSTMENT) with read-only protection.
**Acceptance criteria:**
- Attempting to use more material than available is rejected with an explanatory error message.
- Valid usage decrements stock and logs an `OUT` movement record.
- Stock adjustments update quantity and log an `ADJUSTMENT` movement record.
- Movement records are strictly read-only and cannot be altered or deleted.
**Changelog:**
- 2026-11-13: Phase 6 approved and marked complete. Implemented `StockController.RecordUsage`, Business Rule 1 stock depletion guard, `FormUsageDialog`, `FormAdjustmentDialog`, `UcMaterialUsage`, enhanced `UcStock` with 4-tab ledger and audit trail, and 8 automated tests (total 46 tests passing).

---

### Phase 7: Worker Registration, Daily Attendance & Attendance-Gated Payments
**Goal:** Manage worker types, register daily site workers (*umubyizi*), record attendance with unique constraints, and enforce attendance-gated wage payments.
**Tasks:**
- Build `UcLaborAttendance` view for managing worker types and registering site workers with default and custom daily rates.
- Implement daily attendance recording grid for assigned projects (`PRESENT`, `ABSENT`, `HALF_DAY`).
- Enforce unique constraint: only one attendance record per worker per project per work date.
- Prevent attendance recording for inactive workers.
- Build `UcLaborPayments` for recording daily wage payments linked 1-to-1 to attendance records.
- Implement Business Rule 2: payment creation is strictly blocked unless the worker has an attendance record marked `PRESENT` for that project and date.
- Track `amount_owed`, `amount_paid`, and payment status (`PENDING` vs. `PAID`).
**Acceptance criteria:**
- Duplicate attendance for the same worker on the same date and project is rejected.
- Wage payment cannot be created if the worker was absent or has no attendance record.
- Payment status correctly tracks pending balances and completed payments.
**Changelog:**
- 2026-11-13: Phase 7 approved and marked complete. Implemented `LaborController`, `FormWorkerTypeDialog`, `FormWorkerDialog`, `FormAttendanceDialog`, `FormPaymentDialog`, `UcLabor` with 4 comprehensive management tabs, Business Rule 2 attendance payment gating, and 8 automated tests (total 54 tests passing).

---

### Phase 8: Site Activity Tracking & Progress Monitoring
**Goal:** Implement daily site activity logs with progress percentage tracking strictly bounded between 0 and 100.
**Tasks:**
- Build `UcActivities` user control for logging daily site tasks (title, description, date, progress %).
- Implement Technical Rule 5: validate that progress percentage is an integer strictly between 0 and 100 inclusive.
- Scope activity logging to assigned projects for Site Managers; allow cross-project visibility for Admins.
- Display activity timeline and progress indicator bar within the dashboard and project views.
**Acceptance criteria:**
- Values `< 0` or `> 100` for progress percentage are rejected by the form validator.
- Valid activity records are saved and displayed with date and progress bar.
- Site Managers can only log activities for projects assigned to them.
**Changelog:**
- 2026-11-13: Phase 8 approved and marked complete. Implemented `ActivityController`, `FormActivityDialog`, `UcActivities` with overall progress bar, Technical Rule 5 progress percentage constraints (0-100), and 8 automated tests (total 62 tests passing).

---

### Phase 9: Reports & Data Export (PDF, Excel, CSV)
**Goal:** Develop comprehensive management reports with date range filtering and export capabilities to CSV and PDF/printable formats.
**Tasks:**
- Build `UcReports` with report category selector and date/project filters.
- Implement Material Consumption Report showing usage quantities, unit prices, total costs, and dates.
- Implement Labor Cost Summary Report showing workers, daily rates, amount owed, amount paid, and statuses.
- Implement Stock Movement Audit Trail Report showing all historical adjustments and flows.
- Implement CSV export engine formatting grid data into downloadable `.csv` files.
- Implement formatted PDF / printable document export engine for official submission and review.
**Acceptance criteria:**
- Reports filter accurately by selected project and date range.
- CSV export generates valid, properly formatted comma-separated files opening cleanly in Excel.
- PDF / printable export produces cleanly structured paginated reports with headers, totals, and timestamps.
**Changelog:**
- (empty until review)

---

### Phase 10: End-to-End Verification, Automated Tests & Release Polish
**Goal:** Verify all business and technical validation rules, execute automated test suites, refine UI polish, and finalize documentation.
**Tasks:**
- Write and execute unit tests in `SiteTrack.Tests` covering stock calculation, depletion guard, attendance validation, and payment gating.
- Verify all 5 business validation rules and 5 technical validation rules under edge-case conditions.
- Polish UI layout: clean control margins, responsive resizing, tab order, and shortcut keys.
- Update `VERSION` to `1.0.0` and finalize `docs/CHANGELOG.md` with release notes.
- Complete classroom submission documentation and user setup guide.
**Acceptance criteria:**
- All automated unit and validation tests pass with 100% success rate.
- Application launches cleanly, navigates without visual glitches or unhandled exceptions.
- `VERSION` and `docs/CHANGELOG.md` reflect the completed production release.
**Changelog:**
- (empty until review)

