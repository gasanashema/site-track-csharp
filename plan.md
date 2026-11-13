# SiteTrack Construction Manager — Comprehensive Project Plan

## 1. Project Overview & Objectives
**SiteTrack Construction Manager** is a desktop application developed for AUCA DotNet Lab 3 (Student: Shema Philbert Gasana, ID: 28279). It provides centralized, role-based tracking of multiple construction project sites.

The project translates the system requirements defined in [docs/requirements.md](file:///d:/Projects/AUCA/Class/Dotnet/Lab%203/docs/requirements.md) into a robust **Windows Forms (WinForms)** desktop application using the **Model-View-Controller (MVC)** architectural pattern on the **.NET Framework**, backed by a relational database.

### Core Goals:
1. **Material Resource Management**: Track the full lifecycle of building materials from procurement (`material_purchases`), stock balance per project with weighted average cost calculation (`project_material_stock`), daily site consumption (`material_usage`), and an immutable audit trail (`material_stock_movements`).
2. **Daily Labor Management**: Register casual workers (*umubyizi*), record daily attendance (`worker_attendance`), enforce that wages can only be logged for workers marked `PRESENT`, and track pending vs. paid amounts (`worker_payments`).
3. **Role-Based Site Scoping**: Strict boundary between **Administrator** (cross-project management, master catalogs, user provisioning) and **Site Manager** (confined to assigned projects for daily logs, usage, and payments).
4. **Desktop GUI Experience**: An intuitive WinForms desktop GUI delivering real-time validation, responsive data grids, alert badging for low stock, and exportable reports (CSV, PDF/Excel).

---

## 2. Architecture & Design Pattern (.NET Framework WinForms MVC)

The application adheres strictly to the **Model-View-Controller (MVC)** design pattern:

```
┌─────────────────────────────────────────────────────────────┐
│                         VIEWS (GUI)                         │
│  - FormLogin / FormOtp                                      │
│  - FormMainShell (Dashboard, Navigation, Role Visibility)   │
│  - UserControls / Dialog Forms (Projects, Materials, Labor) │
│  - Exposes Events, Data Binding, UI Validation Highlighting  │
└──────────────────────────▲───────┬──────────────────────────┘
                           │       │ User actions / events
             Updates state │       ▼
┌──────────────────────────┴──────────────────────────────────┐
│                      CONTROLLERS                            │
│  - AuthController, ProjectController, MaterialController,   │
│    LaborController, StockController, ReportController       │
│  - Coordinates business rules, handles UI events, invokes   │
│    repositories/services, returns models/viewmodels to views│
└──────────────────────────▲───────┬──────────────────────────┘
                           │       │ Business Logic / Data Ops
              Domain Data  │       ▼
┌──────────────────────────┴──────────────────────────────────┐
│                   MODELS & DATA ACCESS                      │
│  - Domain Entities (User, Project, Material, Stock, etc.)   │
│  - DTOs / ViewModels for UI binding                         │
│  - Repository Pattern / DbHelper (ADO.NET / Npgsql / SQLite)│
│  - Transaction management for atomic stock & payment updates│
└─────────────────────────────────────────────────────────────┘
```

### Architectural Layering:
- **Models (`SiteTrack.Core.Models`)**: C# POCO classes mapping to the 16 database tables with domain validations and calculated properties (e.g. `QuantityAvailable`, `WeightedAverageUnitPrice`).
- **Data Access (`SiteTrack.Data`)**: Database connection factory (`DatabaseConfig`, `DbConnectionFactory`), SQL scripts/migrations, and repository implementations enforcing ACID transactions.
- **Controllers (`SiteTrack.Controllers`)**: Pure application controllers handling user actions from views, coordinating validation, invoking business rules, managing session context, and updating views.
- **Views (`SiteTrack.UI`)**: WinForms forms and custom UserControls implementing view interfaces (`IProjectView`, `IMaterialView`, `ILaborView`, etc.) enabling clean decoupling, testing, and UI updates.
- **Common / Infrastructure (`SiteTrack.Common`)**: Security helpers (password hashing, OTP generation), notification/event dispatch simulator, export engines (CSV, PDF/Excel).

---

## 3. Scope Definition

### 3.1 In Scope
1. **Direct User Authentication**:
   - Username and password verification with secure hashing (PBKDF2/SHA-256).
   - Instant authentication without secondary OTP challenge (OTP omitted per user directive).
   - Role-based UI activation (Administrator vs. Site Manager).
2. **User Management (Admin)**:
   - Full CRUD for users (`Administrator`, `Site Manager`), account activation/deactivation, duplicate check on username and email.
3. **Project Management & Manager Assignment**:
   - Multi-project registry (Name, Location, Dates, Status: *Planning, Ongoing, Completed, Cancelled*).
   - Assignment of one or more Site Managers per project via `project_managers`.
4. **Material Resource Management**:
   - Material Category & Material master catalogs.
   - Material purchases recording (IN movement, weighted average unit price recalculation, stock balance increment).
   - Material usage recording (OUT movement, stock depletion check, stock balance decrement).
   - Stock adjustments (ADJUSTMENT movement for physical count corrections).
   - Immutable stock movement audit log.
   - Visual alerts when stock falls below `minimum_quantity`.
5. **Labor & Attendance Management**:
   - Worker type catalog with default daily rates.
   - Site worker registration (*umubyizi*) with customizable daily rates.
   - Daily attendance logging with uniqueness constraint (`project_id`, `worker_id`, `work_date`).
   - Strict payment creation rule: payment can only be logged if worker is `PRESENT` on that date.
   - Payment status tracking (`PENDING`, `PAID`) with amount owed vs. paid.
6. **Project Activities**:
   - Daily activity logging with progress percentage strictly bounded between 0 and 100.
7. **Reporting & Data Export**:
   - Material usage report, labor cost report, and stock movement audit report.
   - Date range and project filtering.
   - Export to CSV and PDF/Excel.

### 3.2 Out of Scope
- OTP-based login verification and secondary SMS/email code challenges (omitted per user directive).
- Full corporate accounting, general ledger, tax computation, or formal invoice billing.
- Procurement supplier portal and bidding workflows.
- BIM or CAD integrations.
- Native mobile applications (iOS/Android).
- Hardware GPS tracking of personnel or vehicles.
- Multi-tenant enterprise hosting.
- Live telecom SMS gateway subscriptions (simulated messaging is used).

---

## 4. Users and Roles

| Role | Permissions & Scope |
| :--- | :--- |
| **Administrator** | Full system authority across all active and archived projects. Manages users, projects, site manager assignments, material categories, master material items, worker types, and global financial & material reporting. Can record stock adjustments. |
| **Site Manager** | Scoped strictly to projects to which they are assigned in `project_managers`. Can record daily material purchases, material usage, project activity logs, daily worker attendance, and worker wage payments for their assigned sites. Cannot modify user accounts or master catalogs. |

---

## 5. Data Model & Entity Specifications (16 Entities)

All entities correspond directly to the database schema defined in `docs/requirements.md`:

1. **`User` (`users`)**: `Id`, `FullName`, `Username`, `Email`, `PasswordHash`, `Role` (Admin/SiteManager), `Phone`, `Status` (Active/Inactive), `CreatedAt`, `UpdatedAt`.
2. **`Project` (`projects`)**: `Id`, `ProjectName`, `Location`, `Description`, `StartDate`, `ExpectedEndDate`, `Status` (Planning, Ongoing, Completed, Cancelled), `CreatedBy`, `CreatedAt`, `UpdatedAt`.
3. **`ProjectManager` (`project_managers`)**: `Id`, `ProjectId`, `UserId`, `AssignedDate`, `Status` (Active/Inactive), `CreatedAt`, `UpdatedAt`.
4. **`MaterialCategory` (`material_categories`)**: `Id`, `CategoryName`, `Description`, `CreatedAt`, `UpdatedAt`.
5. **`Material` (`materials`)**: `Id`, `CategoryId`, `MaterialName`, `Unit`, `CurrentPrice`, `Description`, `Status` (Active/Inactive), `CreatedAt`, `UpdatedAt`.
6. **`MaterialPurchase` (`material_purchases`)**: `Id`, `ProjectId`, `MaterialId`, `Quantity`, `UnitPrice`, `TotalPrice`, `SupplierName`, `PurchaseDate`, `RecordedBy`, `CreatedAt`, `UpdatedAt`.
7. **`ProjectMaterialStock` (`project_material_stock`)**: `Id`, `ProjectId`, `MaterialId`, `QuantityAvailable`, `MinimumQuantity`, `AverageUnitPrice`, `CreatedAt`, `UpdatedAt`.
   - *Constraint*: `UNIQUE(project_id, material_id)`.
8. **`MaterialStockMovement` (`material_stock_movements`)**: `Id`, `ProjectId`, `MaterialId`, `MovementType` (IN, OUT, ADJUSTMENT), `Quantity`, `UnitPrice`, `TotalPrice`, `MovementDate`, `Description`, `ReferenceType`, `ReferenceId`, `RecordedBy`, `CreatedAt`, `UpdatedAt`.
9. **`MaterialUsage` (`material_usage`)**: `Id`, `ProjectId`, `MaterialId`, `QuantityUsed`, `UnitPrice`, `TotalCost`, `UsageDate`, `ActivityDescription`, `RecordedBy`, `CreatedAt`, `UpdatedAt`.
10. **`ProjectActivity` (`project_activities`)**: `Id`, `ProjectId`, `ActivityTitle`, `ActivityDescription`, `ActivityDate`, `ProgressPercentage`, `RecordedBy`, `CreatedAt`, `UpdatedAt`.
    - *Constraint*: `CHECK (ProgressPercentage BETWEEN 0 AND 100)`.
11. **`WorkerType` (`worker_types`)**: `Id`, `TypeName`, `DefaultDailyRate`, `Description`, `CreatedAt`, `UpdatedAt`.
12. **`SiteWorker` (`site_workers`)**: `Id`, `WorkerTypeId`, `FullName`, `Phone`, `DailyRate`, `Status` (Active/Inactive), `CreatedAt`, `UpdatedAt`.
13. **`WorkerAttendance` (`worker_attendance`)**: `Id`, `ProjectId`, `WorkerId`, `WorkDate`, `AttendanceStatus` (PRESENT, ABSENT, HALF_DAY), `WorkDescription`, `RecordedBy`, `CreatedAt`, `UpdatedAt`.
    - *Constraint*: `UNIQUE(project_id, worker_id, work_date)`.
14. **`WorkerPayment` (`worker_payments`)**: `Id`, `ProjectId`, `WorkerId`, `AttendanceId`, `WorkDate`, `DailyRate`, `AmountOwed`, `AmountPaid`, `PaymentStatus` (PENDING, PAID), `PaidBy`, `Notes`, `CreatedAt`, `UpdatedAt`.
    - *Constraint*: `UNIQUE(attendance_id)` and Foreign Key to `worker_attendance`.
15. **`OtpVerification` (`otp_verifications`)**: `Id`, `UserId`, `OtpCode`, `ExpiresAt`, `IsUsed`, `CreatedAt`.
16. **`NotificationLog` (`notification_logs`)**: `Id`, `UserId`, `EventType`, `Message`, `Channel` (EMAIL, SMS, SYSTEM), `Status` (DELIVERED, PENDING, FAILED), `CreatedAt`.

---

## 6. Business & Technical Validation Rules

### 6.1 Business Validation Rules
1. **Stock Depletion Protection**: A material usage record MUST be rejected with a user-friendly error if `QuantityUsed > QuantityAvailable` in `project_material_stock`.
2. **Attendance-Gated Wage Payment**: A worker payment record can ONLY be created if there is an existing `worker_attendance` record for that worker, project, and date with `AttendanceStatus == 'PRESENT'`.
3. **Single-Use & Time-Bound OTP**: An OTP code must be rejected if `IsUsed == true` or `DateTime.Now > ExpiresAt`.
4. **Project Scoping Authorization**: A Site Manager cannot view, record, or modify any transaction for a project unless their `UserId` is actively linked to that project in `project_managers`.
5. **Project Deletion / Cancellation Guard**: A project cannot be deleted or cancelled if it has active ongoing stock movements, unconsumed stock balances, or pending worker payments.

### 6.2 Technical Validation Rules
1. **Mandatory Input Validation**: All required form fields must be validated with Visual Studio / WinForms `ErrorProvider` before processing; empty or null entries block submission.
2. **Strict Numeric Formats**: Quantities, unit prices, daily wage rates, and payments must be validated as strictly positive numeric values (`> 0`).
3. **Date Sequence & Formatting**: `ExpectedEndDate` cannot precede `StartDate`. Transaction dates cannot be set in the future.
4. **Uniqueness Constraints**: Duplicate usernames and emails must be checked prior to persisting new or edited user records.
5. **Bounded Percentage Constraint**: Progress percentage in `project_activities` must be an integer between 0 and 100 inclusive.

---

## 7. Edge Cases & Error Handling

- **Database Connection Failure**: Catch ADO.NET connection errors and display a diagnostic connection configuration dialog with retry options.
- **Concurrent Stock Usage**: Wrap stock balance verification and deduction inside a database transaction (`READ COMMITTED` or `SERIALIZABLE`) to prevent race conditions.
- **Deactivated Users**: Prevent login of deactivated accounts even if password matches; revoke active session if deactivated during usage.
- **Zero Stock Purchase**: Creating a purchase for an existing project-material pair that has zero stock must initialize or update the stock record cleanly.
- **Weighted Average Unit Price**:
  $$\text{New Average Price} = \frac{(\text{Current Qty} \times \text{Current Avg Price}) + (\text{New Qty} \times \text{New Unit Price})}{\text{Current Qty} + \text{New Qty}}$$
  Handle division by zero safely if total quantity is 0.

---

## 8. Definition of "Done" for the MVP

The MVP is complete when:
1. The WinForms application compiles with zero errors under .NET Framework 4.8 / .NET modern runtime.
2. The database schema (all 16 tables) is created and seeded with initial demo data (Admin user, sample project, material categories, worker types).
3. The Admin can log in, verify simulated OTP, manage users, projects, site managers, materials, and view cross-project reports.
4. A Site Manager can log in, access only their assigned project(s), record material purchases, record material usage (blocked on insufficient stock), log attendance, and record wage payments (blocked if worker was not present).
5. Reports for material usage, stock movements, and labor costs can be filtered by project and date range and exported to CSV and PDF/printable format.
6. MVC separation is strictly maintained (Form Views contain no raw SQL or direct DB calls; Controllers handle logic; Models represent entities and business data access).

---

## 9. Decisions

- **Authentication Flow**: Direct username and password authentication with secure SHA-256/PBKDF2 hashing. OTP challenge omitted per user directive (2026-11-13) to streamline the desktop evaluation workflow.
- **Architecture**: WinForms desktop application with MVC pattern as mandated by `docs/todo.md`.
- **Database Engine**: Relational database (PostgreSQL with Npgsql or embedded SQLite/LocalDB for portable demonstration), with pure ADO.NET or lightweight repository mapping to ensure full compatibility with .NET Framework WinForms.
- **Reporting Engine**: Built-in CSV export engine and formatted printable / PDF report generator using native WinForms reporting or document generation.

---

## 10. Open Questions

- *Q1: Preferred primary database backend for classroom demonstration* — PostgreSQL (as stated in requirements.md) or SQLite / SQL Server LocalDB for single-click portable evaluation? (Configured to support both via connection string switch).
- *Q2: Password hashing algorithm* — PBKDF2 / BCrypt for production security standards.

---

## 11. Out of Scope

- OTP verification prompts and SMS simulation workflows (excluded per user directive).
- External live SMS gateway subscriptions (e.g. Twilio) requiring credit.
- Direct hardware integration with biometric time-clocks or turnstiles.
- Web or mobile client frontends (desktop WinForms is the target).

