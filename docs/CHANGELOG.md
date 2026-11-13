# Changelog

All notable changes to the **SiteTrack Construction Manager** project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.5.0] - 2026-11-13

### Added
- **User Controller & Administration**: Implemented `UserController` with role authorization checks (Admin-only guards), password complexity enforcement (minimum 6 characters), self-deactivation protection, and user status toggling.
- **User Management View (UcUsers)**: Built desktop user administration panel featuring a styled DataGridView with status highlighting (Green for ACTIVE, Red for INACTIVE), full-text search filtering, and action toolbar.
- **User Creation & Editing Dialog (FormUserDialog)**: Designed modal dialog with comprehensive input validation for name, unique username, unique email, role selection, and optional password update.
- **Project Controller & Manager Assignment**: Built `ProjectController` with scoping logic (Site Managers only see their assigned sites, Admins see all), project CRUD operations, date sequence validation, and site manager assignment.
- **Projects View (UcProjects)**: Developed multi-site project management view with status badges (Ongoing, Planning, Completed, Cancelled), manager list displays, search filtering, and role-based action button permissions.
- **Project Management Dialogs**:
  - `FormProjectDialog`: Creates and edits projects with start date and optional end date controls.
  - `FormAssignManagerDialog`: Assigns and removes site managers dynamically for individual construction projects.
- **Business Rule 5 & Technical Rule 3 Enforcement**: Blocked project deletion when active material stock, usage records, or labor records exist; blocked projects with expected end dates earlier than start dates.
- **Expanded Test Suite**: Added 8 automated tests to `SiteTrack.Tests` covering user creation, non-admin rejection, short password checks, self-deactivation prevention, project timeline validation, site manager assignment/removal, and child record deletion blocking. All 30 tests in the suite passed with 100% success rate.

## [0.4.0] - 2026-11-13

### Added
- **Password Hasher (SHA-256)**: Implemented cryptographic utility `PasswordHasher` in `SiteTrack.Core.Security` for computing and verifying standard SHA-256 hashes.
- **Authentication Controller**: Built `AuthController` in `SiteTrack.Controllers` providing direct credential authentication, deactivated account guards, session tracking (`CurrentUser`, `IsAdmin`), and notification logging.
- **Desktop Login View (FormLogin)**: Developed modern desktop authentication form with credentials input, password masking with toggle, ErrorProvider validation, and one-click demo credentials filling for evaluation.
- **Master Navigation Shell (FormMainShell)**: Created comprehensive WinForms application frame featuring responsive dark slate sidebar, status bar with live user/role/database indicators, and dynamic module loading.
- **Role-Based UI Restructuring**: Implemented strict role separation where Administrators see all modules including `User Management`, and Site Managers operate in site-scoped mode with administrative sections hidden.
- **Session Lifecycle & Logout Loop**: Integrated application lifecycle in `Program.cs` enabling clean logout back to login and application exit.
- **Authentication & Security Test Suite**: Added 7 automated tests to `SiteTrack.Tests` verifying SHA-256 hash vectors, credential validation, deactivated account blocking, admin vs. site manager session states, and logout cleanup. All 22 tests in the suite passed with 100% success rate.

### Changed
- **Direct Login Workflow**: Omitted secondary OTP verification dialogs per user directive, streamlining the desktop login flow directly to the role-based main shell.

## [0.3.0] - 2026-11-13

### Added
- **Database Connection Factory**: Implemented `IDbConnectionFactory` and `DbConnectionFactory` with ADO.NET connection testing, provider resolution, and fallback to `SiteTrackDataStore`.
- **Relational Data Storage Engine**: Built thread-safe relational data store `SiteTrackDataStore` with auto-increment ID generation, transactional integrity, and seed initialization.
- **Reference Seed Data (002_seed_data.sql)**: Seeded default Admin (`admin`), Site Managers (`sitemgr`, `aline.mgr`), 3 sample construction projects, 6 material categories, 10 materials, 6 worker types, and 10 site workers (*umubyizi* & craftsmen).
- **Core Repository Interfaces**: Implemented `IUserRepository`, `IProjectRepository`, `IMaterialRepository`, `IStockRepository`, `ILaborRepository`, and `IReportRepository` in `SiteTrack.Core.Interfaces`.
- **Concrete Repositories**:
  - `UserRepository`: User CRUD, role management, OTP storage/expiry validation, notification logging, and duplicate check guards.
  - `ProjectRepository`: Project lifecycle management, Site Manager assignment (`project_managers`), project scoping by manager, and active child deletion guards.
  - `MaterialRepository`: Category and material catalog operations with foreign key integrity guards.
  - `StockRepository`: Purchase recording with weighted average unit price formula, stock depletion guard (Business Rule 1), physical stock adjustments, and immutable movement logging (IN, OUT, ADJUSTMENT).
  - `LaborRepository`: Worker catalog, daily attendance logging (unique per project/worker/date), and attendance-gated wage payment creation (Business Rule 2).
  - `ReportRepository`: Total material expenditure and labor cost aggregations.
- **Data Layer Integration Test Suite**: Expanded `SiteTrack.Tests` from 8 to 15 automated tests verifying connection testing, seed data loading, user uniqueness guards, manager project scoping, weighted average price recalculation, stock depletion rejection, and attendance payment gating. All 15 tests passed with 100% success rate.

## [0.2.0] - 2026-11-13

### Added
- **Solution & Project Layering**: Created `SiteTrack.sln` containing `SiteTrack.Core`, `SiteTrack.Data`, `SiteTrack.Controllers`, `SiteTrack.UI`, and `SiteTrack.Tests`.
- **Database DDL Schema (16 Tables)**: Implemented complete relational schema script `docs/001_initial_schema.sql` (and embedded copy in `SiteTrack.Data/Scripts/001_initial_schema.sql`) covering all 16 tables: `users`, `projects`, `project_managers`, `material_categories`, `materials`, `material_purchases`, `project_material_stock`, `material_stock_movements`, `material_usage`, `project_activities`, `worker_types`, `site_workers`, `worker_attendance`, `worker_payments`, `otp_verifications`, and `notification_logs`.
- **Domain Entity POCOs**: Implemented all 16 C# entity models in `SiteTrack.Core.Models` with navigation helper properties, business calculations (e.g. `IsLowStock`, `TotalStockValue`, `BalanceDue`, `IsFullyPaid`), and default values.
- **Validation & Flow Types**: Implemented `Enums.cs` for role/status types, `Result.cs` & `Result<T>` operation response wrappers, and `ValidationException.cs`.
- **Base MVC Controller**: Implemented `BaseController` with static application session state tracking (`CurrentUser`, `IsAuthenticated`, `IsAdmin`).
- **Automated Domain Test Suite**: Implemented `SiteTrack.Tests` with 8 automated unit tests verifying default states, low stock triggers, weighted average formulas, attendance gating, and payment balance logic. All 8 tests pass with 100% success rate.
- **Entry Point UI Shell**: Created WinForms project `SiteTrack.UI` with initial `Program.cs`.

## [0.1.0] - 2026-11-13

### Added
- **Project Foundation & Requirements Analysis**: Synthesized system requirements from AUCA DotNet Lab 3 specification (`docs/requirements.md` and `docs/todo.md`) into core project documentation.
- **Root Version File**: Created `VERSION` file tracking semantic version `0.1.0`.
- **Master Project Plan**: Created `plan.md` defining system goals, MVC architecture on .NET Framework WinForms, in-scope/out-of-scope modules, all 16 database entities, 5 business validation rules, 5 technical validation rules, role matrix, and decisions.
- **Detailed Implementation Plan**: Created `docs/IMPLEMENTATION_PLAN.md` with multi-tier solution layout (`SiteTrack.Core`, `SiteTrack.Data`, `SiteTrack.Controllers`, `SiteTrack.UI`, `SiteTrack.Tests`), database DDL roadmap, milestone checklists, and UX guardrails.
- **Phase-by-Phase Plan**: Created `docs/phase-by-phase-plan.md` breaking implementation down into 10 structured, verifiable phases with concrete goals, tasks, acceptance criteria, and changelog sections.
- **Agent Skill Integration**: Integrated workspace skills (`versioning-workflow`, `grill-me`, and `phase-by-phase`) for continuous tracking, interactive review, and strict phase-driven execution.

