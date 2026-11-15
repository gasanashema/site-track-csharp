# Changelog

All notable changes to the **SiteTrack Construction Manager** project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.3] - 2026-11-15

### Added
- **Comprehensive Root Documentation (`README.md`)**: Authored complete, university-grade repository documentation for AUCA DotNet Lab 3 including:
  - System abstract, problem statement, and solution architecture.
  - Strict Model-View-Controller (MVC) 5-tier project breakdown.
  - Complete 16-table relational database schema matrix with primary/foreign key mappings and unique/check constraints.
  - All 5 Business Validation Rules and 5 Technical Validation Rules detailed with enforcement mechanisms.
  - Step-by-step instructions on running via precompiled Release executable, PowerShell/MSBuild compilation, Visual Studio, automated test runner, and cross-platform Docker container.
  - Comprehensive operational module walkthroughs (Multi-Project, Weighted Average Stock Purchases, Usage Guards, Attendance-Gated Wages, 0-100% Progress Tracking, and CSV/Print Reports).
  - Evaluation credentials and FAQ/troubleshooting guide.

## [1.0.2] - 2026-11-15

### Fixed
- **Sidebar Navigation Docking Z-Order Collision**: Fixed internal docking layout bug inside `pnlSidebar` where `pnlNavButtons` (`DockStyle.Fill`) was docked at index 2 (before `pnlBrand` at index 0), causing `pnlNavButtons` to start at `Y = 0` and position its top buttons ("Dashboard" and "Projects") directly underneath the 85px brand logo panel.
- **Deterministic Sidebar Z-Order**: Enforced explicit reverse z-order docking hierarchy via `pnlSidebar.Controls.SetChildIndex`:
  - `pnlBrand`: Set to `ChildIndex = 2` (docks 1st, claiming `Y: 0..85` for the brand logo header).
  - `btnNavSignOut`: Set to `ChildIndex = 1` (docks 2nd, claiming `Y: Bottom-44..Bottom`).
  - `pnlNavButtons`: Set to `ChildIndex = 0` (docks 3rd, filling `Y: 85..Bottom-44` strictly below the logo panel).
  - Enabled `pnlNavButtons.AutoScroll = true` for responsive vertical scrolling.

## [1.0.1] - 2026-11-15

### Fixed
- **WinForms Content Docking & Z-Order Collision**: Fixed critical docking layout bug in `FormMainShell` where `pnlHeader.BringToFront()` and `pnlSidebar.BringToFront()` inadvertently pushed `pnlContent` to higher z-index, causing `pnlContent` to dock across the entire window at `(0, 0)` and render underneath the topbar (top 70px) and sidebar (left 240px).
- **Deterministic Docking Hierarchy**: Enforced explicit reverse z-order docking hierarchy via `Controls.SetChildIndex`:
  - `pnlSidebar`: Docked 1st (`ChildIndex = 3`), occupying the entire left column `[0, 0, 240, Height]`.
  - `statusStrip`: Docked 2nd (`ChildIndex = 2`), occupying bottom row `[240, Height-23, Width-240, 23]`.
  - `pnlHeader`: Docked 3rd (`ChildIndex = 1`), occupying top row `[240, 0, Width-240, 70]`.
  - `pnlContent`: Docked 4th (`ChildIndex = 0`), filling the remaining client viewport `[240, 70, Width-240, Height-93]` with zero overlap.
- **Content Padding & Responsiveness**: Removed duplicate 24px container padding (`pnlContent.Padding = new Padding(0)`) to allow module controls (`UcProjects`, `UcStock`, `UcLabor`, `UcActivities`, `UcReports`, `UcUsers`) to render flush with their own clean padding without clipping or double-margin waste. Enabled `pnlContent.AutoScroll = true`.
- **Interactive Executive Dashboard**: Enhanced initial landing screen (`Dashboard`) with dynamic live summary cards (Active Construction Sites, Master Materials Catalog, Low Stock Alerts requiring action, Registered Site Craftsmen), a Quick Action navigation toolbar, and an Active Sites overview table.

## [1.0.0] - 2026-11-13

### Added
- **Full End-to-End System Verification**: Validated complete lifecycle integration covering user login, multi-project administration, material procurement, moving weighted average cost accounting, material usage with real-time stock depletion guards, craftsman registration, daily attendance logging, attendance-gated wage payment vouchers, daily site activity monitoring with progress percentage boundaries, and management report exports.
- **Rule Verification Test Expansion**: Added 6 comprehensive automated tests to `SiteTrack.Tests` explicitly targeting:
  - Technical Rule 1: Whitespace and empty string input rejection across projects, materials, and workers.
  - Technical Rule 2: Zero and negative numeric value rejection for quantities, unit prices, and wage rates.
  - Technical Rule 3: Single-day task acceptance and invalid reverse date sequence rejection.
  - Business Rule 4: Cross-domain Site Manager project scoping enforcement.
  - Business Rule 5: Project deletion blocking when active material stock or wage records exist.
  - End-to-End Lifecycle Integration: Full multi-module transaction verification.
- **Final Test Suite**: Expanded automated test suite to 76 tests with 100% pass rate in both Debug and Release configurations.
- **Cross-Platform Containerization**: Added multi-stage `Dockerfile` and `docker-compose.yml` for automated headless builds and test execution across Linux and Windows container hosts.
- **Comprehensive User & Administrator Guide**: Created `docs/USER_GUIDE.md` detailing system architecture, 16-table relational database schema, seed credentials, rule definitions, step-by-step walkthroughs, and build/run instructions.

## [0.9.5] - 2026-11-13

### Added
- **Management Reporting Controller (ReportController)**: Implemented specialized reporting and analytics generation covering Material Consumption, Labor Wage & Attendance Summaries, Stock Movement Audit Trails, and Cross-Project Executive Overviews.
- **Reporting DTO Models**: Defined `MaterialConsumptionReportItem`, `LaborWageReportItem`, `StockMovementReportItem`, and `ProjectExecutiveSummary` in `SiteTrack.Core.Models`.
- **RFC 4180 Compliant CSV Exporter (CsvExporter)**: Built robust CSV generator with proper quoting/escaping, column header mapping, and UTF-8 with BOM encoding for seamless Microsoft Excel compatibility on Windows.
- **Paginated Print & PDF Report Engine (PrintReportHelper)**: Implemented a print engine using .NET `PrintDocument` and `PrintPreviewDialog` with landscape orientation, custom page headers, summary metadata, column width scaling, alternating row fills, and automatic page breaks.
- **Desktop Reporting & Analytics Dashboard (UcReports)**:
  - Interactive multi-report dashboard with project selector and date range filters (defaulting to comprehensive historical spans).
  - Key metric summary cards dynamically updating per report (e.g., Total Spend, Consumed Quantity, Distinct Materials; Total Disbursed, Outstanding Balances, Present Headcount).
  - Clean `DataGridView` rendering with currency, numeric, and date formatting.
  - Action buttons for instant **Export to CSV** (with file dialog and auto-open prompt) and **Print / Preview PDF** (with native zoomable preview dialog).
- **Main Shell Integration**: Connected navigation menu item `Reports & Export` to dynamically load `UcReports`.
- **Expanded Automated Test Suite**: Added 8 automated tests to `SiteTrack.Tests` covering Material Consumption aggregation, Labor Wage balance tracking, Stock Movement history, Executive Summary KPI computation, Site Manager scoping enforcement, and CSV format/escape/BOM disk saving. Total test suite reached 70 passing tests with 100% success rate.

## [0.9.0] - 2026-11-13

### Added
- **Site Activity Controller (ActivityController)**: Implemented activity logging, project scoping validation (Site Managers scoped to assigned sites, Admins cross-project), and latest progress tracking.
- **Technical Rule 5 Enforcement**: Constrained milestone progress percentage strictly between `0` and `100` inclusive with boundary checks (0% and 100% accepted; `< 0` and `> 100` rejected).
- **Desktop Activity Logging Views**:
  - `UcActivities`: Interactive daily site activity dashboard with project selector, search filtering, overall milestone completion progress bar, summary metrics (latest reported progress, milestone count, last activity date), and styled timeline DataGridView.
  - `FormActivityDialog`: Modal dialog for logging site tasks with live progress slider/progress bar synchronization, title/description validation, and Technical Rule 5 error handling.
- **Main Shell Integration**: Enabled `Site Activities` module navigation in `FormMainShell.cs`.
- **Expanded Automated Test Suite**: Added 8 automated tests to `SiteTrack.Tests` covering valid activity recording, Technical Rule 5 negative progress rejection, excessive (> 100) progress rejection, boundary values (0% and 100%), empty title rejection, site manager scoping, unassigned project blocking, and progress timeline ordering. Total test suite reached 62 passing tests with 100% success rate.

## [0.8.0] - 2026-11-13

### Added
- **Labor Controller & Work Management (LaborController)**: Implemented worker skill categories CRUD, site worker registration with custom daily wage rates, daily attendance tracking, and attendance-gated wage payments.
- **Attendance-Gated Wage Payments (Business Rule 2)**: Enforced strict gating blocking daily wage payments unless the worker has an attendance record marked `PRESENT` for that project and date. Supported partial payments and balance settlement tracking.
- **Unique Attendance Constraint Enforcement**: Enforced single attendance record per worker per project per work date with upsert capability, and strictly blocked attendance logging for inactive workers.
- **Desktop Labor Views**:
  - `UcLabor`: Multi-tab labor management control featuring:
    - Tab 1: Daily Attendance & Wage Allocation (with project and date filters, summary cards for workers logged, present, and daily wage liability).
    - Tab 2: Worker Registry (*Aba-byizi* and craftsmen directory with category and status filters, and modal edit dialogs).
    - Tab 3: Wage Payments Ledger (voucher history, total disbursed, outstanding liabilities, and balance settlement).
    - Tab 4: Skill Categories & Standard Rates (trade benchmarks and rate configurations).
  - `FormWorkerTypeDialog`: Modal for creating and editing skill categories with standard rates.
  - `FormWorkerDialog`: Modal for registering site workers with category selection, contact info, and custom daily rates.
  - `FormAttendanceDialog`: Modal for recording attendance status (`PRESENT`, `ABSENT`, `HALF_DAY`) and assigned tasks.
  - `FormPaymentDialog`: Modal for recording wage payments with live Business Rule 2 eligibility indicators and balance due tracking.
- **Expanded Automated Test Suite**: Added 8 automated tests to `SiteTrack.Tests` covering worker type CRUD, non-admin blocking, duplicate trade rejection, worker registration with custom rates, daily attendance upsert, inactive worker attendance rejection, Business Rule 2 absent payment rejection, and attendance-gated payment balance tracking. Total test count reached 54 tests passing with 100% success rate.

## [0.7.0] - 2026-11-13

### Added
- **Material Usage & Stock Depletion Guard (Business Rule 1)**: Implemented `RecordUsage` in `StockController`, enforcing strict stock depletion validation preventing material usage when `quantity_used > quantity_available`. Automatically applies weighted average unit cost and decrements project stock balance.
- **Immutable Movement Audit Logging**: Recorded immutable `OUT` movements for material usage with activity descriptions, recorded by user ID, and timestamp.
- **Physical Inventory Stock Adjustment**: Implemented `RecordStockAdjustment` in `StockController` allowing Administrators to reconcile physical inventory counts with calculated deltas and immutable `ADJUSTMENT` movement logging.
- **Material Usage View (UcMaterialUsage)**: Developed dedicated user control featuring project selector, date range filtering, search filter, consumption metrics (entries, total cost, distinct materials), and styled DataGridView.
- **Material Usage Dialog (FormUsageDialog)**: Created modal dialog with live stock availability indicator, real-time depletion validation warnings, and site activity inputs.
- **Physical Adjustment Dialog (FormAdjustmentDialog)**: Created admin modal dialog with live delta computation (surplus vs. deficit) and audit reason requirements.
- **Enhanced Stock Inventory View (UcStock)**: Added dedicated tabs for "Daily Material Usage (Outflow Ledger)" and "Stock Movement Audit Trail (Read-Only)", with color-coded movement type pills (Green for IN, Red for OUT, Blue for ADJUSTMENT) and direct action buttons for Purchase, Usage, and Adjustment.
- **Expanded Automated Test Suite**: Added 8 automated tests to `SiteTrack.Tests` covering valid usage deduction, Business Rule 1 stock depletion rejection, non-positive quantity checks, empty activity description guards, site manager project scoping for usages, physical inventory adjustment with delta logging, non-admin adjustment rejection, and multi-type movement audit trail verification. All 46 tests in the suite passed with 100% success rate.

## [0.6.0] - 2026-11-13

### Added
- **Material Category & Catalog Management (MaterialController)**: Implemented category CRUD, master material item creation, reference unit pricing, negative price rejection, and category deletion guards against linked active materials.
- **Material Catalog Views**:
  - `UcMaterials`: Master catalog management control with searchable categorized materials list and dedicated Category Management tab.
  - `FormCategoryDialog`: Category creation and edit modal with duplicate name prevention.
  - `FormMaterialDialog`: Material registration dialog enforcing category selection, positive price, unit of measurement, and initial status.
- **Stock Tracking & Purchase Workflow (StockController)**:
  - Atomic purchase logging updating `material_purchases` and `project_material_stock`.
  - Recalculates weighted average unit price dynamically using `((old_qty * old_avg) + (new_qty * new_price)) / (old_qty + new_qty)`.
  - Records immutable `IN` transaction in `material_stock_movements`.
  - Scoping validation: Site Managers can only record purchases for projects assigned to them.
- **Stock Management & Purchase Views**:
  - `UcStock`: Real-time stock inventory dashboard displaying quantity available, minimum thresholds, weighted unit prices, total valuation per item, and low-stock highlighting (`⚠️ LOW STOCK` in bold red).
  - `FormPurchaseDialog`: Material purchase entry modal with project scoping, supplier input, quantity, unit price, and total expenditure calculation.
- **Phase 5 Automated Test Suite**: Added 8 automated tests to `SiteTrack.Tests` covering category CRUD, deletion prevention for linked materials, master material validation, purchase recording, atomic weighted average price recalculation, and project-scoped purchase access. All 38 tests in the suite passed with 100% success rate.

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

