# SiteTrack Construction Manager 🏗️

[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen.svg)]()
[![Tests](https://img.shields.io/badge/Tests-76%20Passed%20%2F%200%20Failed-success.svg)]()
[![Platform](https://img.shields.io/badge/Platform-.NET%20Framework%204.0%20%2F%204.8-blue.svg)]()
[![Architecture](https://img.shields.io/badge/Architecture-MVC%20WinForms-orange.svg)]()
[![Database](https://img.shields.io/badge/Database-16%20Relational%20Tables-purple.svg)]()
[![Version](https://img.shields.io/badge/Release-v1.0.3-blue.svg)]()

> **Academic Submission:** Adventist University of Central Africa (AUCA)  
> **Course:** DotNet Lab 3  
> **Author / Student:** Shema Philbert Gasana  
> **Student ID:** 28279  
> **Current Version:** `1.0.3`  

---

## Table of Contents
1. [Overview & Abstract](#1-overview--abstract)
2. [System Architecture (Strict MVC)](#2-system-architecture-strict-mvc)
3. [Relational Database Schema (16 Tables)](#3-relational-database-schema-16-tables)
4. [Enforced Business & Technical Validation Rules](#4-enforced-business--technical-validation-rules)
5. [Evaluation Credentials & Role Scoping](#5-evaluation-credentials--role-scoping)
6. [Core Functional Modules](#6-core-functional-modules)
7. [Step-by-Step Instructions: How to Run](#7-step-by-step-instructions-how-to-run)
8. [Automated Verification & Test Runner](#8-automated-verification--test-runner)
9. [Running with Docker (Cross-Platform)](#9-running-with-docker-cross-platform)
10. [Troubleshooting & FAQ](#10-troubleshooting--faq)
11. [Project Directory Map](#11-project-directory-map)
12. [Release & Version History](#12-release--version-history)

---

## 1. Overview & Abstract

**SiteTrack Construction Manager** is an enterprise-grade Windows desktop application built in C# following the **Model-View-Controller (MVC)** architectural pattern on the Microsoft .NET Framework.

In the construction industry across Rwanda and East Africa, general contractors frequently manage multiple geographically dispersed active project sites. Operational challenges include:
- **Informal Daily Labor Practices**: Daily casual laborers (*Aba-byizi*) are often paid informally with cash without verifiable attendance tracking, leading to phantom worker claims and wage disputes.
- **Untracked Material Waste & Depletion**: Materials purchased for one site are consumed without real-time tracking, resulting in stockouts and cost overruns.
- **Inaccurate Material Costing**: Material prices fluctuate across supplier invoices, making single-price tracking financially inaccurate.
- **Lack of Role Isolation**: Site managers frequently possess unrestricted access to other project budgets or system configurations.

SiteTrack solves these operational challenges by introducing:
1. **Multi-Project Administration with Scoping**: Administrators manage all sites centrally; Site Managers are scoped strictly to their assigned construction sites.
2. **Moving Weighted Average Stock Accounting**: Material purchases automatically update real-time stock balances and calculate moving weighted average unit prices.
3. **Stock Depletion Guard (Business Rule 1)**: Material usage entries are strictly rejected if the requested quantity exceeds the current on-hand site stock balance.
4. **Attendance-Gated Daily Wage Payments (Business Rule 2)**: Wage vouchers can strictly only be issued for workers marked `PRESENT` in the attendance register on that date.
5. **Daily Site Operations & Milestone Progress (Technical Rule 5)**: Daily task logs track milestone progress percentage constrained between `0` and `100` with interactive visual indicators.
6. **Multi-Format Management Reporting**: Instant report generation for material consumption, labor liabilities, and complete stock movements with **RFC 4180 CSV export (UTF-8 BOM)** and **paginated print / PDF preview**.

---

## 2. System Architecture (Strict MVC)

The solution is partitioned into five decoupled, cohesive assemblies targeting .NET Framework:

```text
SiteTrack.sln
│
├── src/SiteTrack.Core/        ─── [MODEL LAYER]
│   ├── Models/                    Domain POCOs (User, Project, Material, Stock, Labor, Activity, Report)
│   ├── Enums.cs                   Strongly typed statuses (ProjectStatus, AttendanceStatus, PaymentStatus)
│   ├── Result.cs                  Functional Result and Result<T> error handling contracts
│   ├── CsvExporter.cs             RFC 4180 compliant CSV generator with UTF-8 BOM
│   └── Security/                  SHA-256 cryptographic password hashing engine
│
├── src/SiteTrack.Data/        ─── [PERSISTENCE LAYER]
│   ├── Connection/                DbConnectionFactory & thread-safe relational datastore
│   ├── Repositories/              Repository Pattern (User, Project, Material, Stock, Labor, Report)
│   └── Scripts/                   001_initial_schema.sql & 002_seed_data.sql runners
│
├── src/SiteTrack.Controllers/ ─── [CONTROLLER LAYER]
│   ├── BaseController.cs          Session management, role checks, and project scoping
│   ├── AuthController.cs          Direct credentials authentication & session initialization
│   ├── UserController.cs          Admin user provisioning & deactivation guards
│   ├── ProjectController.cs       Site creation, date checks (TR-3), and deletion guards (BR-5)
│   ├── MaterialController.cs      Categories and master catalog administration
│   ├── StockController.cs         Purchases, weighted averages, and usage guards (BR-1)
│   ├── LaborController.cs         Craftsmen registry, attendance, and gated payments (BR-2)
│   ├── ActivityController.cs      Daily operations and progress percentage boundaries (TR-5)
│   └── ReportController.cs        Scoped analytics and financial aggregations
│
├── src/SiteTrack.UI/          ─── [VIEW LAYER - WinForms GUI]
│   ├── Forms/
│   │   ├── FormLogin.cs           Authentication dialog with AUCA quick-fill demo buttons
│   │   ├── FormMainShell.cs       Modern dark-slate navigation shell, topbar & executive dashboard
│   │   └── Dialogs/               Modal entry forms with Windows ErrorProviders
│   ├── Controls/
│   │   ├── UcProjects.cs          Multi-project registry, filters & site manager mappings
│   │   ├── UcStock.cs             Master stock, purchases, usage & audit trail tabbed ledger
│   │   ├── UcMaterialUsage.cs     Daily material consumption with live depletion alerts
│   │   ├── UcLabor.cs             Craftsmen directory, daily attendance & wage payment vouchers
│   │   ├── UcActivities.cs        Daily site activities timeline with progress slider
│   │   ├── UcReports.cs           Management analytics, KPI cards, CSV export & print preview
│   │   └── UcUsers.cs             Admin user provisioning, credentials & status toggles
│   └── Export/
│       └── PrintReportHelper.cs   Paginated landscape print document & preview engine
│
└── src/SiteTrack.Tests/       ─── [VERIFICATION LAYER]
    └── Program.cs                 Standalone test runner executing 76 automated test cases
```

---

## 3. Relational Database Schema (16 Tables)

SiteTrack implements a normalized relational database schema comprising **16 tables** with primary keys, foreign keys, unique constraints, and check constraints:

| # | Table Name | Key Columns | Relational Constraints & Rules |
| :---: | :--- | :--- | :--- |
| **1** | `users` | `id`, `username`, `email`, `password_hash`, `role`, `status` | Unique username & email; SHA-256 password hash. |
| **2** | `projects` | `id`, `project_name`, `location`, `start_date`, `expected_end_date`, `status` | Status: `Planning`, `Ongoing`, `Completed`, `Cancelled`. |
| **3** | `project_managers` | `id`, `project_id`, `user_id`, `assigned_date`, `status` | Many-to-Many junction mapping Site Managers to Sites. |
| **4** | `material_categories` | `id`, `category_name`, `description` | Master grouping (Cement, Steel, Masonry, Plumbing). |
| **5** | `materials` | `id`, `category_id`, `material_name`, `unit`, `current_price` | FK to `material_categories`. Current benchmark unit price. |
| **6** | `material_purchases` | `id`, `project_id`, `material_id`, `quantity`, `unit_price`, `total_price` | Captures invoice price and supplier at purchase time. |
| **7** | `project_material_stock` | `id`, `project_id`, `material_id`, `quantity_available`, `average_unit_price` | **`UNIQUE(project_id, material_id)`**; moving weighted average cost. |
| **8** | `material_stock_movements` | `id`, `project_id`, `material_id`, `movement_type`, `quantity`, `reference_id` | **Immutable audit ledger**: `IN` (purchase), `OUT` (usage), `ADJUSTMENT`. |
| **9** | `material_usage` | `id`, `project_id`, `material_id`, `quantity_used`, `activity_description` | Daily consumption logged by Site Managers. |
| **10** | `project_activities` | `id`, `project_id`, `activity_title`, `progress_percentage` | **`CHECK(progress_percentage BETWEEN 0 AND 100)`**. |
| **11** | `worker_types` | `id`, `type_name`, `default_daily_rate`, `description` | Trade benchmarks (*Master Mason*, *Scaffolder*, *Helper*). |
| **12** | `site_workers` | `id`, `worker_type_id`, `full_name`, `phone`, `daily_rate`, `status` | Craftsmen registry with custom daily wage rates. |
| **13** | `worker_attendance` | `id`, `project_id`, `worker_id`, `work_date`, `attendance_status` | **`UNIQUE(project_id, worker_id, work_date)`**; `PRESENT`/`ABSENT`/`HALF_DAY`. |
| **14** | `worker_payments` | `id`, `project_id`, `worker_id`, `attendance_id`, `amount_owed`, `amount_paid` | **`UNIQUE(attendance_id)`**; FK to `worker_attendance`. |
| **15** | `otp_verifications` | `id`, `user_id`, `otp_code`, `expires_at`, `is_used` | Security verification table (direct password login active per lab scope). |
| **16** | `notification_logs` | `id`, `user_id`, `event_type`, `message`, `channel`, `status` | System audit and event notification logs. |

### Database Initialization Scripts
- [`docs/001_initial_schema.sql`](file:///d:/Projects/AUCA/Class/Dotnet/Lab%203/docs/001_initial_schema.sql): Complete DDL script creating all 16 tables, primary keys, foreign keys, and indexes.
- [`docs/002_seed_data.sql`](file:///d:/Projects/AUCA/Class/Dotnet/Lab%203/docs/002_seed_data.sql): Seed script providing default administrative users, sample construction projects in Kigali, standard trade categories, material items, and initial inventories.

---

## 4. Enforced Business & Technical Validation Rules

All 10 rules from the AUCA specification are enforced at the controller level and covered by automated regression tests:

### Business Validation Rules
- **Business Rule 1 (Stock Depletion Guard)**: Material usage is strictly blocked if `QuantityUsed > QuantityAvailable`. The system rejects the transaction with a clear error and prevents negative inventory.
- **Business Rule 2 (Attendance-Gated Wage Payments)**: A wage voucher can **only** be created if the worker has an attendance record marked `PRESENT` for that project and date. Disallows paying absent workers.
- **Business Rule 3 (Direct Credentials Enforcement)**: Validates username, checks against deactivated accounts, and verifies cryptographic SHA-256 password hash. *(OTP entry excluded per student lab instruction).*
- **Business Rule 4 (Site Manager Project Scoping)**: Site Managers cannot view, record, or modify purchases, usages, attendance, payments, activities, or reports for projects to which they are not assigned.
- **Business Rule 5 (Project Deletion Guard)**: Projects cannot be cancelled or deleted if active material stock, historical stock movements, or wage payment liabilities exist.

### Technical Validation Rules
- **Technical Rule 1 (Required Field Validation)**: All mandatory input fields (names, codes, titles, dates) cannot be empty or whitespace; validated with Windows `ErrorProvider`.
- **Technical Rule 2 (Positive Numeric Constraints)**: Quantities, unit prices, daily rates, and financial amounts accept only positive numbers (`> 0`).
- **Technical Rule 3 (Date Sequence Validation)**: Project `expected_end_date` cannot precede `start_date`. Same-day tasks (`start_date == expected_end_date`) are valid.
- **Technical Rule 4 (Uniqueness Constraints)**: Usernames and emails are strictly validated for uniqueness across all user accounts prior to insertion.
- **Technical Rule 5 (Progress Percentage Boundary)**: Milestone progress percentage must be an integer strictly between `0` and `100` inclusive (`0 <= progress <= 100`).

---

## 5. Evaluation Credentials & Role Scoping

The application comes pre-loaded with demonstration accounts. The login dialog also features **Quick Login** buttons to fill these credentials with a single click:

| Role | Username | Password | Operational Access Scope |
| :--- | :--- | :--- | :--- |
| **System Administrator** | `admin` | `Admin@123` | **Full cross-project access**: User administration, master material catalog, craftsman benchmarks, project creation & assignment, physical stock reconciliation, and global reporting. |
| **Site Operations Manager** | `sitemgr` | `Admin@123` | **Scoped access**: Limited strictly to assigned sites (*Project 1: Kigali Heights Expansion*, *Project 2: AUCA Science Complex*). Cannot modify unassigned sites or access user administration. |

---

## 6. Core Functional Modules

### 1. Interactive Executive Dashboard
- **Welcome Banner**: Displays active user full name, role, current system date, and total active sites.
- **Live Summary Metric Cards**:
  - **Active Sites**: Total projects scoped to the active user.
  - **Master Materials**: Catalog items registered across categories.
  - **Low Stock Alerts**: Real-time count of materials where `QuantityAvailable <= MinimumQuantity`.
  - **Site Craftsmen**: Total workforce registered in the system.
- **Quick Action Bar**: Shortcuts to jump directly to any operational workflow.
- **Active Construction Sites Overview**: Real-time summary table of sites, locations, start dates, and projected completion.

### 2. Multi-Site Project Administration (`UcProjects`)
- Create new projects with location, description, and dates.
- Edit project metadata and update status (`Planning`, `Ongoing`, `Completed`, `Cancelled`).
- Assign or revoke Site Managers through the `project_managers` mapping.
- Protected by Business Rule 5: Cannot delete projects with active stock or wage records.

### 3. Material Catalog & Moving Weighted Average Pricing (`UcStock`, `UcMaterials`)
- Master material categories and individual items with units (Bags, Tonnes, Meters).
- Record incoming supplier purchases (quantity, invoice unit price, supplier name, date).
- **Moving Weighted Average Formula**: Automatically calculates the updated moving unit cost on every purchase:
  $$\text{New Average} = \frac{(\text{Current Qty} \times \text{Current Avg Price}) + (\text{New Qty} \times \text{Purchase Price})}{\text{Current Qty} + \text{New Qty}}$$
- Records an immutable `IN` movement entry.

### 4. Daily Material Usage & Physical Inventory Reconciliation (`UcMaterialUsage`)
- Log daily material consumption per site with specific task descriptions.
- **Business Rule 1 Guard**: Instantly warns if requested quantity exceeds available stock and rejects the transaction.
- Decrements `project_material_stock` balance and records an immutable `OUT` movement.
- **Physical Reconciliation Dialog**: Allows Administrators to record physical inventory counts (`ADJUSTMENT` movement) to reconcile surplus or deficit.

### 5. Labor Attendance & Attendance-Gated Wage Payments (`UcLabor`)
- **Craftsman Registry**: Register site workers (*Aba-byizi*, Masons, Scaffolders, Electricians) with trade categories and custom daily wage rates.
- **Daily Attendance**: Record attendance (`PRESENT`, `ABSENT`, `HALF_DAY`) per worker per site per date with upsert capability.
- **Attendance-Gated Wage Vouchers (Business Rule 2)**: Pay daily wages only if the worker is recorded as `PRESENT`. If `ABSENT`, the payment button is disabled with an explanatory indicator.
- Tracks `AmountOwed`, `AmountPaid`, and `BalanceDue` for partial payment tracking.

### 6. Daily Site Activities & Milestone Progress (`UcActivities`)
- Log daily operational milestones (activity title, description, date, progress percentage).
- **Technical Rule 5 Guard**: Interactive slider and progress bar bounded strictly between 0% and 100%. Values `< 0` or `> 100` are blocked.
- Displays overall site progress percentage bar at the top of the dashboard.

### 7. Management Reporting, CSV Export & PDF Print Preview (`UcReports`)
- Select from 4 specialized reports:
  1. *Material Consumption & Expenditure Report*
  2. *Labor Wages & Attendance Summary Report*
  3. *Stock Movement Audit Ledger (IN/OUT/ADJUSTMENT)*
  4. *Executive Project Cost & Progress Overview*
- Filter by project and start/end dates.
- Dynamic KPI summary metric cards (Total Spend, Disbursed Wages, Outstanding Balance, Quantities).
- **Export to CSV**: Generates RFC 4180 compliant CSV files with **UTF-8 BOM** for immediate opening in Microsoft Excel.
- **Print / Preview PDF**: Native landscape paginated print engine with zoomable preview dialog.

---

## 7. Step-by-Step Instructions: How to Run

### System Prerequisites
- **Operating System:** Windows 10, Windows 11, or Windows Server (x64 / x86).
- **.NET Framework:** .NET Framework 4.0, 4.5, 4.7.2, or 4.8 (pre-installed on all modern Windows installations).
- **Build Tools (optional, for compiling from source):** MSBuild (included with .NET Framework at `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe` or Visual Studio 2019/2022).

---

### Option 1: Running the Precompiled Release Executable (Fastest)

The solution is already compiled in **Release** mode and ready to run immediately:

1. Open **PowerShell** or **Command Prompt** in the project root directory:
   ```powershell
   cd "d:\Projects\AUCA\Class\Dotnet\Lab 3"
   ```
2. Launch the desktop executable:
   ```powershell
   .\src\SiteTrack.UI\bin\Release\SiteTrack.exe
   ```
3. The **SiteTrack Authentication** window will appear on your desktop.
4. Click the demo button **Admin (admin)** or enter:
   - **Username:** `admin`
   - **Password:** `Admin@123`
5. Click **Sign In** to access the full system.

---

### Option 2: Building and Running via PowerShell / MSBuild

To clean, compile, and execute the application from source:

1. Open **PowerShell** as Administrator or Standard User.
2. Navigate to the project root directory:
   ```powershell
   cd "d:\Projects\AUCA\Class\Dotnet\Lab 3"
   ```
3. Compile the solution in **Release** mode using the .NET 4.0 MSBuild engine:
   ```powershell
   C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe SiteTrack.sln /t:Rebuild /p:Configuration=Release
   ```
   *(Verify that the build outputs: `0 Warning(s), 0 Error(s)`).*
4. Launch the application:
   ```powershell
   Start-Process -FilePath ".\src\SiteTrack.UI\bin\Release\SiteTrack.exe"
   ```

---

### Option 3: Opening and Running in Visual Studio

1. Open **Visual Studio** (2019, 2022, or newer).
2. Click **File → Open → Project/Solution** and select [`SiteTrack.sln`](file:///d:/Projects/AUCA/Class/Dotnet/Lab%203/SiteTrack.sln).
3. In the Solution Explorer, right-click on **`SiteTrack.UI`** and select **Set as Startup Project**.
4. Choose configuration **Release** or **Debug** and platform **Any CPU**.
5. Press **F5** (or click **Start**) to build and run the application with the debugger attached.

---

## 8. Automated Verification & Test Runner

SiteTrack includes a comprehensive automated test runner (`SiteTrack.Tests`) that executes unit tests, business rule validations, and an end-to-end multi-module transaction scenario.

### Running the Test Suite via Command Line:
```powershell
# Run the compiled test runner
.\src\SiteTrack.Tests\bin\Release\SiteTrack.Tests.exe
```

### Expected Output (76 Tests, 100% Pass Rate):
```text
=================================================
SiteTrack Construction Manager — Test Suite Runner
=================================================
[PASS] User Default Status is ACTIVE
[PASS] Project Default Status is Planning
[PASS] Stock Low Stock Indicator
[PASS] Weighted Average Unit Price Calculation
[PASS] Worker Attendance PRESENT status gate
[PASS] Worker Payment Balance Due Calculation
[PASS] Result and Result<T> contract
[PASS] Data Layer: Connection Factory TestConnection
[PASS] Data Layer: Initial Seed Data is Loaded
[PASS] Data Layer: User Repository Duplicate Username/Email Guard
[PASS] Data Layer: Project Scoping by Site Manager
[PASS] Data Layer: Stock Purchase and Weighted Average Price Calculation
[PASS] Data Layer: Stock Usage Depletion Rejection Guard (Business Rule 1)
[PASS] Data Layer: Attendance-Gated Payment Creation (Business Rule 2)
[PASS] Security: PasswordHasher Computes Consistent SHA-256 Hash
[PASS] Auth: Controller Rejects Missing Username or Password
[PASS] Auth: Controller Rejects Nonexistent User
[PASS] Auth: Controller Rejects Wrong Password
[PASS] Auth: Controller Blocks Deactivated Accounts
[PASS] Auth: Successful Admin Authentication Sets Session and Role
[PASS] Auth: Successful Site Manager Authentication Sets Scoped Role
[PASS] Auth: Logout Clears Application Session
[PASS] User Controller: Admin Can Create New User
[PASS] User Controller: Non-Admin Blocked from Creating User
[PASS] User Controller: Reject Short Password (< 6 chars)
[PASS] User Controller: Prevent Self-Deactivation of Logged-in Admin
[PASS] Project Controller: Admin Can Create Project with Valid Dates
[PASS] Project Controller: Technical Rule 3 Rejects EndDate Before StartDate
[PASS] Project Controller: Admin Can Assign and Remove Site Manager
[PASS] Project Controller: Business Rule 5 Blocks Deleting Project with Active Child Records
[PASS] Material Controller: Admin Can Create Material Category
[PASS] Material Controller: Non-Admin Blocked from Creating Category
[PASS] Material Controller: Block Deleting Category with Linked Materials
[PASS] Material Controller: Admin Can Create Master Material with Positive Price
[PASS] Material Controller: Reject Material with Negative Price
[PASS] Stock Controller: Purchase Atomically Updates Stock and Calculates Weighted Average
[PASS] Stock Controller: Site Manager Cannot Purchase For Unassigned Project
[PASS] Stock Controller: Low Stock Detection on Project Stock
[PASS] Usage Controller: Valid Usage Decrements Stock and Records OUT Movement
[PASS] Usage Controller: Business Rule 1 Rejects Usage Exceeding Stock
[PASS] Usage Controller: Reject Non-Positive Usage Quantity
[PASS] Usage Controller: Reject Empty Activity Description
[PASS] Usage Controller: Site Manager Cannot Record Usage For Unassigned Project
[PASS] Stock Adjustment: Admin Can Record Physical Inventory Reconciliation
[PASS] Stock Adjustment: Non-Admin Blocked From Recording Stock Adjustment
[PASS] Audit Trail: Stock Movement Ledger Captures IN, OUT, and ADJUSTMENT
[PASS] Labor Controller: Admin Can Create Worker Type With Benchmark Rate
[PASS] Labor Controller: Non-Admin Blocked From Creating Worker Type
[PASS] Labor Controller: Reject Duplicate Worker Type Name
[PASS] Labor Controller: Admin Can Register Site Worker With Custom Daily Rate
[PASS] Labor Controller: Record Daily Attendance PRESENT and Upsert Logic
[PASS] Labor Controller: Prevent Attendance For Inactive Worker
[PASS] Labor Controller: Business Rule 2 Rejects Wage Payment When Attendance is ABSENT
[PASS] Labor Controller: Business Rule 2 Allows Wage Payment for PRESENT Worker and Tracks Balance
[PASS] Activity Controller: Admin Can Record Valid Activity and Progress Percentage
[PASS] Activity Controller: Technical Rule 5 Rejects Negative Progress Percentage (< 0)
[PASS] Activity Controller: Technical Rule 5 Rejects Excessive Progress Percentage (> 100)
[PASS] Activity Controller: Technical Rule 5 Accepts Boundary Percentages (0% and 100%)
[PASS] Activity Controller: Reject Empty Activity Title
[PASS] Activity Controller: Site Manager Can Record Activity For Assigned Project
[PASS] Activity Controller: Site Manager Blocked From Recording Activity For Unassigned Project
[PASS] Activity Controller: Timeline Returns Ordered Activities and Latest Progress
[PASS] Report Controller: Material Consumption Report Aggregates Quantities and Costs
[PASS] Report Controller: Labor Wage Report Returns Present Workers and Correct Balances
[PASS] Report Controller: Stock Movement Audit Report Returns Complete Movement History
[PASS] Report Controller: Project Executive Summary Computes Totals and Milestones
[PASS] Report Controller: Site Manager Cannot Generate Report For Unassigned Project
[PASS] CSV Exporter: Format Standard Headers and Rows
[PASS] CSV Exporter: Properly Escapes Commas, Quotes, and Newlines
[PASS] CSV Exporter: Save UTF-8 With BOM File to Disk
[PASS] Technical Rule 1: Rejects Empty or Whitespace Required Fields
[PASS] Technical Rule 2: Rejects Zero and Negative Numeric Fields
[PASS] Technical Rule 3: Accepts Same-Day Dates But Rejects EndDate Before StartDate
[PASS] Business Rule 4: Site Manager Scoping Strictly Enforced Across All Domains
[PASS] Business Rule 5: Cannot Cancel or Delete Project With Active Movements or Stock
[PASS] End-to-End Lifecycle Integration: Project, Purchase, Usage, Labor, Activity, and Reporting
-------------------------------------------------
Results: 76 PASSED, 0 FAILED
=================================================
```

---

## 9. Running with Docker (Cross-Platform)

For cross-platform environments (e.g., Linux or CI/CD pipelines), SiteTrack provides a `Dockerfile` using Mono and a `docker-compose.yml` service:

### Build and Run Tests in Container:
```bash
docker-compose up --build sitetrack-tests
```

### Standalone Docker Build:
```bash
docker build -t sitetrack-app .
docker run --rm sitetrack-app
```

---

## 10. Troubleshooting & FAQ

#### Q1: "The process cannot access the file ... because it is being used by another process."
**Answer:** An existing instance of `SiteTrack.exe` is currently running. Stop it via PowerShell before rebuilding:
```powershell
Get-Process -Name "SiteTrack" -ErrorAction SilentlyContinue | Stop-Process -Force
```

#### Q2: CSV export characters look strange in Microsoft Excel.
**Answer:** SiteTrack exports all CSV files using **UTF-8 with BOM** (`0xEF, 0xBB, 0xBF`). This ensures Excel on Windows automatically detects the UTF-8 encoding and renders currency signs and accents properly without manual import wizard configuration.

#### Q3: How do I test the Attendance-Gated Payment rule (Business Rule 2)?
**Answer:**
1. Log in as `sitemgr` or `admin`.
2. Go to **Labor & Attendance**.
3. In Tab 1 (*Daily Attendance*), mark a worker as `ABSENT`.
4. Click **Pay Daily Wage**. Notice that the payment action is disabled and displays an error indicating that payments can strictly only be issued for workers marked `PRESENT`.
5. Change the attendance status to `PRESENT`. Click **Pay Daily Wage**; the payment voucher dialog opens and calculates balance due.

#### Q4: How do I test the Stock Depletion Guard (Business Rule 1)?
**Answer:**
1. Go to **Materials & Stock** → Tab 3 (*Daily Material Usage*).
2. Click **+ Record Material Usage**.
3. Select *Portland Cement 42.5N* (note current available stock balance).
4. Enter a quantity exceeding the available stock balance (e.g., 9999).
5. The system immediately rejects the transaction with an alert preventing negative inventory.

---

## 11. Project Directory Map

```text
SiteTrack Construction Manager
├── SiteTrack.sln                 # Visual Studio Solution definition
├── VERSION                       # Current Semantic Version (1.0.2)
├── Dockerfile                    # Container definition for cross-platform build & test
├── docker-compose.yml            # Multi-service container orchestration
├── README.md                     # Comprehensive project documentation & run guide
│
├── docs/                         # Specifications & Planning
│   ├── requirements.md           # System Requirements Specification (SRS)
│   ├── todo.md                   # Lab 3 assignment instructions
│   ├── IMPLEMENTATION_PLAN.md    # 10 completed milestone checklists
│   ├── phase-by-phase-plan.md    # 10 completed phase records
│   ├── CHANGELOG.md              # Semantic version history (v0.1.0 -> v1.0.2)
│   ├── USER_GUIDE.md             # Administrator & User Manual
│   ├── 001_initial_schema.sql    # 16-table relational schema DDL
│   └── 002_seed_data.sql         # Seed dataset script
│
└── src/                          # Source Code
    ├── SiteTrack.Core/           # Layer 1: Domain Models, POCOs, CSV Engine
    ├── SiteTrack.Data/           # Layer 2: Relational Persistence, Repositories
    ├── SiteTrack.Controllers/    # Layer 3: MVC Controllers & Validation Rules
    ├── SiteTrack.UI/             # Layer 4: Windows Forms Desktop Views & Shell
    └── SiteTrack.Tests/          # Layer 5: Automated Verification Suite (76 Tests)
```

---

## 12. Release & Version History

All changes adhere to [Semantic Versioning](https://semver.org/spec/v2.0.0.html) and [Keep a Changelog](https://keepachangelog.com/en/1.0.0/):

| Version | Release Date | Key Deliverables & Milestones |
| :---: | :---: | :--- |
| **`v1.0.3`** | 2026-11-15 | **Documentation**: Authored comprehensive root `README.md` covering architecture, 16-table relational schema, validation rules, module walkthroughs, and step-by-step execution guides. |
| **`v1.0.2`** | 2026-11-15 | **Bugfix**: Enforced reverse z-order docking inside `pnlSidebar` to prevent top navigation buttons from rendering underneath the logo panel. |
| **`v1.0.1`** | 2026-11-15 | **Bugfix & UI Polish**: Fixed WinForms reverse z-order docking hierarchy in `FormMainShell`, preventing content panel from sliding behind the sidebar and header. Enhanced `Dashboard` with live KPI cards, quick actions, and active sites overview. |
| **`v1.0.0`** | 2026-11-13 | **Production Release**: Final end-to-end integration verification, automated test expansion to 76 tests (100% pass), Dockerfile & docker-compose containerization, and comprehensive `USER_GUIDE.md`. |
| **`v0.9.5`** | 2026-11-13 | **Phase 9**: Management reporting controller, RFC 4180 CSV export with UTF-8 BOM, and paginated landscape print / PDF preview engine. |
| **`v0.9.0`** | 2026-11-13 | **Phase 8**: Daily site activity logging, milestone timeline, and Technical Rule 5 progress percentage constraints (0-100%). |
| **`v0.8.0`** | 2026-11-13 | **Phase 7**: Craftsman registry, daily attendance tracking, and Business Rule 2 attendance-gated wage payment vouchers. |
| **`v0.7.0`** | 2026-11-13 | **Phase 6**: Material consumption, Business Rule 1 stock depletion guard, physical inventory adjustments, and immutable movement audit log. |
| **`v0.6.0`** | 2026-11-13 | **Phase 5**: Material catalog, purchase recording, and moving weighted average price calculation. |
| **`v0.5.0`** | 2026-11-13 | **Phase 4**: User administration (Admin only) and multi-project management with Site Manager site scoping. |
| **`v0.4.0`** | 2026-11-13 | **Phase 3**: User authentication dialog with demo quick-fills, password hashing (SHA-256), and main navigation shell. |
| **`v0.3.0`** | 2026-11-13 | **Phase 2**: Connection factory, relational datastore, repository pattern implementations, and schema/seed migration runners. |
| **`v0.2.0`** | 2026-11-13 | **Phase 1**: Visual Studio solution architecture, 16 POCO entity models, Result pattern, and SQL schema definitions. |
| **`v0.1.0`** | 2026-11-13 | **Initialization**: Requirements analysis, implementation plan, and project scaffolding. |

---

*SiteTrack Construction Manager is developed for AUCA DotNet Lab 3 examination and evaluation. All rights reserved.*
