# SiteTrack Construction Manager — Implementation Plan

## 1. Executive Summary & Architecture Blueprint
- **Project Name**: SiteTrack Construction Manager (Desktop WinForms MVC)
- **Institution / Course**: Adventist University of Central Africa (AUCA) — DotNet (Lab 3)
- **Author**: Shema Philbert Gasana (Student ID: 28279)
- **Target Platform**: Windows Forms (.NET Framework 4.8 / C#)
- **Architecture Pattern**: Model-View-Controller (MVC) with Repository Pattern
- **Database Engine**: Relational Database (PostgreSQL / SQLite interchangeable via `IDbConnectionFactory`)
- **Source Plan**: [plan.md](../plan.md) and [docs/requirements.md](requirements.md)

---

## 2. Solution Structure & Layering

```text
SiteTrack/
├── SiteTrack.sln
├── SiteTrack.Core/                  # Model & Domain Layer
│   ├── Models/                      # 16 Domain Entities (POCOs)
│   │   ├── User.cs
│   │   ├── Project.cs
│   │   ├── ProjectManager.cs
│   │   ├── MaterialCategory.cs
│   │   ├── Material.cs
│   │   ├── MaterialPurchase.cs
│   │   ├── ProjectMaterialStock.cs
│   │   ├── MaterialStockMovement.cs
│   │   ├── MaterialUsage.cs
│   │   ├── ProjectActivity.cs
│   │   ├── WorkerType.cs
│   │   ├── SiteWorker.cs
│   │   ├── WorkerAttendance.cs
│   │   ├── WorkerPayment.cs
│   │   ├── OtpVerification.cs
│   │   └── NotificationLog.cs
│   ├── ViewModels/                  # DTOs for Grid & Report displays
│   │   ├── StockBalanceSummaryVm.cs
│   │   ├── LaborCostSummaryVm.cs
│   │   └── AttendanceRosterVm.cs
│   ├── Interfaces/                  # Repository & Service Interfaces
│   │   ├── IUserRepository.cs
│   │   ├── IProjectRepository.cs
│   │   ├── IMaterialRepository.cs
│   │   ├── IStockRepository.cs
│   │   ├── ILaborRepository.cs
│   │   └── IReportRepository.cs
│   └── Validation/                  # Business & Technical Validation helpers
│       ├── BusinessRulesValidator.cs
│       └── TechnicalRulesValidator.cs
├── SiteTrack.Data/                  # Data Access Layer
│   ├── Connection/
│   │   ├── IDbConnectionFactory.cs
│   │   └── DbConnectionFactory.cs
│   ├── Repositories/
│   │   ├── UserRepository.cs
│   │   ├── ProjectRepository.cs
│   │   ├── MaterialRepository.cs
│   │   ├── StockRepository.cs
│   │   ├── LaborRepository.cs
│   │   └── ReportRepository.cs
│   ├── Migrations/                  # DDL Schemas & Seed Data scripts
│   │   ├── 001_initial_schema.sql
│   │   └── 002_seed_data.sql
│   └── DbHelper.cs                  # Safe Parameterized ADO.NET query execution
├── SiteTrack.Controllers/           # Controller Layer (MVC)
│   ├── BaseController.cs
│   ├── AuthController.cs
│   ├── UserController.cs
│   ├── ProjectController.cs
│   ├── MaterialController.cs
│   ├── StockController.cs
│   ├── LaborController.cs
│   └── ReportController.cs
├── SiteTrack.UI/                    # View Layer (Windows Forms)
│   ├── Interfaces/                  # View Interfaces (for Decoupled MVC)
│   │   ├── ILoginView.cs
│   │   ├── IMainShellView.cs
│   │   ├── IProjectView.cs
│   │   ├── IMaterialView.cs
│   │   ├── ILaborView.cs
│   │   └── IReportView.cs
│   ├── Forms/
│   │   ├── FormLogin.cs             # Credentials + Role detection
│   │   ├── FormOtpDialog.cs         # OTP Verification prompt
│   │   ├── FormMainShell.cs         # Master Navigation Frame + Role scoping
│   │   └── Dialogs/                 # Modal dialogs for CRUD inputs
│   ├── Controls/                    # UserControls embedded in MainShell
│   │   ├── UcDashboard.cs           # Metrics, alerts, summary charts
│   │   ├── UcProjects.cs            # Project CRUD & Manager Assignment
│   │   ├── UcMaterials.cs           # Master categories, materials, purchases
│   │   ├── UcStock.cs               # Stock balance, low stock alert, adjustments
│   │   ├── UcMaterialUsage.cs       # Daily usage logging with stock guard
│   │   ├── UcLaborAttendance.cs     # Worker roster & daily attendance grid
│   │   ├── UcLaborPayments.cs       # Attendance-linked wage payments
│   │   ├── UcActivities.cs          # Site progress logging (0-100%)
│   │   ├── UcUsers.cs               # Admin user management
│   │   └── UcReports.cs             # Filterable reports & export (PDF/Excel/CSV)
│   └── Program.cs                   # Application entry point & session setup
└── SiteTrack.Tests/                 # Unit & Integration Tests
    ├── ValidationTests.cs
    ├── StockCalculationTests.cs
    └── AttendancePaymentRuleTests.cs
```

---

## 3. Database Schema & Persistence Strategy

### 16 Normalized Database Tables
1. `users` (id, full_name, username, email, password_hash, role, phone, status, created_at, updated_at)
2. `projects` (id, project_name, location, description, start_date, expected_end_date, status, created_by, created_at, updated_at)
3. `project_managers` (id, project_id, user_id, assigned_date, status, created_at, updated_at)
4. `material_categories` (id, category_name, description, created_at, updated_at)
5. `materials` (id, category_id, material_name, unit, current_price, description, status, created_at, updated_at)
6. `material_purchases` (id, project_id, material_id, quantity, unit_price, total_price, supplier_name, purchase_date, recorded_by, created_at, updated_at)
7. `project_material_stock` (id, project_id, material_id, quantity_available, minimum_quantity, average_unit_price, created_at, updated_at)
   - *Key Rule*: `UNIQUE(project_id, material_id)`
8. `material_stock_movements` (id, project_id, material_id, movement_type, quantity, unit_price, total_price, movement_date, description, reference_type, reference_id, recorded_by, created_at, updated_at)
9. `material_usage` (id, project_id, material_id, quantity_used, unit_price, total_cost, usage_date, activity_description, recorded_by, created_at, updated_at)
10. `project_activities` (id, project_id, activity_title, activity_description, activity_date, progress_percentage, recorded_by, created_at, updated_at)
11. `worker_types` (id, type_name, default_daily_rate, description, created_at, updated_at)
12. `site_workers` (id, worker_type_id, full_name, phone, daily_rate, status, created_at, updated_at)
13. `worker_attendance` (id, project_id, worker_id, work_date, attendance_status, work_description, recorded_by, created_at, updated_at)
    - *Key Rule*: `UNIQUE(project_id, worker_id, work_date)`
14. `worker_payments` (id, project_id, worker_id, attendance_id, work_date, daily_rate, amount_owed, amount_paid, payment_status, paid_by, notes, created_at, updated_at)
    - *Key Rule*: `UNIQUE(attendance_id)` with FK constraint to `worker_attendance(id)`
15. `otp_verifications` (id, user_id, otp_code, expires_at, is_used, created_at)
16. `notification_logs` (id, user_id, event_type, message, channel, status, created_at)

---

## 4. Implementation Checklist by Milestones

### Milestone 1: Core Foundation & Data Access
- [x] Initialize repository and project structure (`SiteTrack.sln`).
- [x] Implement SQL schema creation scripts for all 16 tables.
- [x] Implement database connection factory and test connection handler.
- [x] Implement seed script with default Administrator account (`admin`/`Admin@123`), sample projects, worker types, and material categories.
- [x] Implement POCO entity models in `SiteTrack.Core.Models`.

### Milestone 2: Authentication, Security & Role Scoping
- [x] Implement `IUserRepository` and `UserRepository` with password hashing (PBKDF2/SHA-256).
- [x] Implement `AuthController` handling direct login credentials, active status validation, and session context creation.
- [x] Build `FormLogin` with username/password inputs and error provider validation (direct authentication; OTP excluded per user directive).
- [x] Implement session context storing active user ID, role, and assigned projects.

### Milestone 3: Shell Navigation & User Management
- [x] Build `FormMainShell` with modern sidebar navigation and role-based access control.
- [x] Implement `UserController` and `UcUsers` (Admin only) for user CRUD, role assignment, and account activation/deactivation.
- [x] Implement validation preventing duplicate usernames and emails.

### Milestone 4: Project Management & Site Assignment
- [x] Implement `ProjectRepository` and `ProjectController`.
- [x] Build `UcProjects` for creating/editing projects (Planning, Ongoing, Completed, Cancelled).
- [x] Build Site Manager assignment UI (`project_managers` mapping).
- [x] Enforce project scoping: Site Managers see only assigned projects in project selectors.
- [x] Implement deletion guards preventing removal of projects with active transactions.

### Milestone 5: Material Catalogs & Purchase Management
- [x] Implement `MaterialRepository` and `MaterialController`.
- [x] Build `UcMaterials` for managing material categories and individual materials.
- [x] Implement purchase recording dialog (`material_purchases`).
- [x] Implement transaction logic:
  - Add `material_purchases` entry.
  - Insert or update `project_material_stock` balance.
  - Calculate weighted average unit price.
  - Insert immutable `material_stock_movements` record of type `IN`.

### Milestone 6: Stock Tracking, Adjustments & Usage Depletion
- [x] Build `UcStock` with color-coded highlighting for materials below `minimum_quantity`.
- [x] Implement stock adjustment dialog for physical inventory reconciliation (`ADJUSTMENT` movement).
- [x] Build `UcMaterialUsage` for Site Managers to log daily material consumption.
- [x] Implement Business Rule 1: Reject usage if `quantity_used > quantity_available`.
- [x] Implement usage transaction: decrement stock, record `material_usage`, record `OUT` movement.

### Milestone 7: Labor Registration, Attendance & Gated Wage Payments
- [x] Implement worker type management (`worker_types`) and worker registration (`site_workers`).
- [x] Build `UcLaborAttendance` for recording daily worker attendance (`PRESENT`, `ABSENT`, `HALF_DAY`).
- [x] Enforce unique attendance constraint (`project_id`, `worker_id`, `work_date`).
- [x] Build `UcLaborPayments` for recording daily wage payments.
- [x] Implement Business Rule 2: Strictly block payment creation unless worker is `PRESENT` on that date.
- [x] Track `amount_owed` vs. `amount_paid` and status (`PENDING` vs. `PAID`).

### Milestone 8: Site Activities & Daily Progress
- [x] Build `UcActivities` for logging site operations.
- [x] Implement Technical Rule 5: Progress percentage constrained strictly between 0 and 100.
- [x] Filter activity logs by project and date.

### Milestone 9: Reporting, Analytics & Data Export
- [x] Build `UcReports` with report selector and date range filters:
  - Material Consumption & Expenditure Report
  - Labor Wage & Attendance Summary Report
  - Stock Movement Audit Log Report
  - Cross-Project Executive Dashboard
- [x] Implement CSV export engine.
- [x] Implement PDF / printable document export engine.

### Milestone 10: Verification, Documentation & Release
- [ ] Run automated tests verifying validation rules and calculations.
- [ ] Update `VERSION`, `docs/CHANGELOG.md`, and `docs/phase-by-phase-plan.md`.
- [ ] Prepare User Guide & database setup manual for AUCA Lab 3 submission.

---

## 5. Design & UX Guardrails
- **Color Palette & Styling**: Professional dark slate sidebar (`#1E293B`), clean content canvas (`#F8FAFC`), crisp data grid formatting with alternating row colors.
- **Visual Feedback**:
  - `ErrorProvider` for input field validation errors.
  - Color-coded badges for status (e.g. `Ongoing` = Green, `Planning` = Blue, `Completed` = Gray, `Cancelled` = Red).
  - Amber/Red highlight rows on stock grids when `quantity_available < minimum_quantity`.
  - Confirmation dialogs for destructive actions.
- **Responsiveness**: Docking and anchoring used systematically across all WinForms controls.
- **Status Bar**: Display current logged-in user, active role, database connection status, and current date/time.

---

## 6. Verification & Quality Assurance Protocol
- **Unit / Logic Tests**:
  - Weighted average price calculation with varying quantities and prices.
  - Stock usage rejection when insufficient balance.
  - Payment rejection when worker status is `ABSENT` or missing.
  - Percentage validation (reject `< 0` and `> 100`).
  - OTP expiration check (reject expired or reused OTPs).
- **Data Integrity Tests**:
  - Attempting to delete a project with child records must be blocked.
  - Attempting duplicate attendance on the same day for a worker must throw a clean UI validation error.

