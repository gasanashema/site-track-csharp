# SiteTrack Construction Manager — User & Administrator Guide

**Course:** DotNet Lab 3  
**Institution:** Adventist University of Central Africa (AUCA)  
**Student Name:** Shema Philbert Gasana  
**Student ID:** 28279  
**Version:** 1.0.0  
**Application Architecture:** Layered Model-View-Controller (MVC) Desktop System on .NET Framework 4.0 / 4.8  

---

## 1. System Overview & Architecture

**SiteTrack Construction Manager** is an enterprise-grade Windows Forms desktop application developed in C# following strict **Model-View-Controller (MVC)** architectural standards. Designed specifically for construction contracting firms in Rwanda and East Africa, SiteTrack enables real-time oversight of multi-site operations, material inventories, weighted average cost accounting, daily labor attendance, wage disbursements, and site progress tracking.

### Solution Structure

The system is organized into five decoupled assemblies:

| Project | Role | Key Responsibilities |
| :--- | :--- | :--- |
| **`SiteTrack.Core`** | Model Layer | Domain entity POCOs (`User`, `Project`, `Material`, `Stock`, `Labor`, `Activity`, `Report`), Enums, Result wrappers, Password Hasher (SHA-256), and RFC 4180 CSV Exporter. |
| **`SiteTrack.Data`** | Persistence Layer | Connection factory (`DbConnectionFactory`), thread-safe relational datastore, repository implementations (`UserRepository`, `ProjectRepository`, `MaterialRepository`, `StockRepository`, `LaborRepository`, `ReportRepository`), and SQL schema/seed migration runners. |
| **`SiteTrack.Controllers`** | Controller Layer | Business logic controllers enforcing validation rules, role scoping, weighted average price calculations, depletion guards, and attendance gates. |
| **`SiteTrack.UI`** | View Layer | Modern desktop GUI featuring a responsive slate navigation shell (`FormMainShell`), modular dashboard controls (`UcProjects`, `UcStock`, `UcMaterialUsage`, `UcLabor`, `UcActivities`, `UcReports`, `UcUsers`), modal dialogs, and native paginated print engines. |
| **`SiteTrack.Tests`** | Verification Layer | Standalone test runner executing 76 automated unit, regression, and end-to-end integration tests verifying all 5 business rules and 5 technical rules. |

---

## 2. Database Schema & Setup

The underlying relational schema consists of **16 normalized tables** with primary keys, foreign keys, unique constraints, and check constraints:

1. `users` (id, full_name, username, email, password_hash, role, phone, status, created_at, updated_at)
2. `projects` (id, project_name, location, description, start_date, expected_end_date, status, created_by, created_at, updated_at)
3. `project_managers` (id, project_id, user_id, assigned_date, status, created_at, updated_at)
4. `material_categories` (id, category_name, description, created_at, updated_at)
5. `materials` (id, category_id, material_name, unit, current_price, description, status, created_at, updated_at)
6. `material_purchases` (id, project_id, material_id, quantity, unit_price, total_price, supplier_name, purchase_date, recorded_by, created_at, updated_at)
7. `project_material_stock` (id, project_id, material_id, quantity_available, minimum_quantity, average_unit_price, created_at, updated_at) — `UNIQUE(project_id, material_id)`
8. `material_stock_movements` (id, project_id, material_id, movement_type, quantity, unit_price, total_price, movement_date, description, reference_type, reference_id, recorded_by, created_at, updated_at)
9. `material_usage` (id, project_id, material_id, quantity_used, unit_price, total_cost, usage_date, activity_description, recorded_by, created_at, updated_at)
10. `project_activities` (id, project_id, activity_title, activity_description, activity_date, progress_percentage, recorded_by, created_at, updated_at)
11. `worker_types` (id, type_name, default_daily_rate, description, created_at, updated_at)
12. `site_workers` (id, worker_type_id, full_name, phone, daily_rate, status, created_at, updated_at)
13. `worker_attendance` (id, project_id, worker_id, work_date, attendance_status, work_description, recorded_by, created_at, updated_at) — `UNIQUE(project_id, worker_id, work_date)`
14. `worker_payments` (id, project_id, worker_id, attendance_id, work_date, daily_rate, amount_owed, amount_paid, payment_status, paid_by, notes, created_at, updated_at) — `UNIQUE(attendance_id)`
15. `otp_verifications` (id, user_id, otp_code, expires_at, is_used, created_at)
16. `notification_logs` (id, user_id, event_type, message, channel, status, created_at)

### Initial SQL Scripts
- [`docs/001_initial_schema.sql`](file:///d:/Projects/AUCA/Class/Dotnet/Lab%203/docs/001_initial_schema.sql): Creates all 16 tables, foreign keys, unique constraints, and check constraints.
- [`docs/002_seed_data.sql`](file:///d:/Projects/AUCA/Class/Dotnet/Lab%203/docs/002_seed_data.sql): Populates the default users, sample projects in Kigali, master material catalog, standard craftsman categories, and initial stock balances.

### Default User Credentials

| Username | Password | Role | Description |
| :--- | :--- | :--- | :--- |
| **`admin`** | `Admin@123` | **Admin** | Full system administrator. Cross-project visibility, user administration, trade category rates, master catalog, project creation, and physical stock reconciliation. |
| **`sitemgr`** | `Admin@123` | **SiteManager** | Site Operations Manager. Scoped strictly to assigned projects (Project 1: Kigali Heights Expansion, Project 2: AUCA Science Complex). |

*(Note: Per explicit lab instruction, OTP entry was excluded from the desktop UI to streamline demonstration workflows; direct password authentication is enforced).*

---

## 3. Business & Technical Validation Rules

The application strictly implements all 10 core constraints specified in the System Requirements Specification:

### Business Validation Rules
1. **Business Rule 1 (Stock Depletion Guard)**: Material usage is strictly rejected if `quantity_used > quantity_available`. Prevents negative physical inventory.
2. **Business Rule 2 (Attendance-Gated Wage Payments)**: A wage payment voucher can **only** be created if the worker has an attendance record marked `PRESENT` for that project and date. Disallows paying absent workers.
3. **Business Rule 3 (Direct Credentials Enforcement)**: Direct authentication verifies username, SHA-256 hashed password, and blocks deactivated user accounts immediately.
4. **Business Rule 4 (Site Manager Project Scoping)**: Site Managers cannot view, record, or modify purchases, usages, attendance, payments, activities, or reports for projects they are not assigned to.
5. **Business Rule 5 (Project Deletion Guard)**: A project cannot be deleted or cancelled if it has active material stock, historical stock movements, or wage payment records.

### Technical Validation Rules
1. **Technical Rule 1 (Required Field Validation)**: All mandatory input fields (names, codes, titles, dates) are validated as non-empty before processing, with visual feedback provided via Windows `ErrorProvider`.
2. **Technical Rule 2 (Positive Numeric Constraints)**: Quantities, unit prices, daily rates, and financial amounts accept only positive numbers (`> 0`).
3. **Technical Rule 3 (Date Sequence Validation)**: Project `expected_end_date` cannot precede `start_date`. Same-day tasks (`start_date == expected_end_date`) are accepted.
4. **Technical Rule 4 (Uniqueness Constraints)**: Usernames and emails are validated for uniqueness across all user accounts prior to insertion.
5. **Technical Rule 5 (Progress Percentage Boundary)**: Milestone progress percentage is validated strictly as an integer between `0` and `100` inclusive (`0 <= progress <= 100`).

---

## 4. End-to-End Operational Walkthrough

### 1. User Authentication
1. Launch `SiteTrack.exe`.
2. Enter username (`admin` or `sitemgr`) and password (`Admin@123`).
3. Click **Sign In**. The system authenticates credentials, establishes a secure application session, and opens the main navigation shell styled with role-specific menu items.

### 2. Multi-Project Administration (`UcProjects`)
- **Admins** can create new projects, edit project dates, update statuses (`Planning`, `Ongoing`, `Completed`, `Cancelled`), and assign or revoke Site Managers.
- Site Managers view their assigned projects in a clean cards/grid layout.

### 3. Material Catalog & Weighted Average Purchases (`UcStock`, `UcMaterials`)
- **Master Catalog**: Admins configure material categories (e.g., Cement, Masonry, Steel, Plumbing) and items with benchmark prices and units (Bags, Tonnes, Meters).
- **Purchases**: Users record incoming supplies (quantity, supplier name, invoice unit price, purchase date).
- **Weighted Average Calculation**: The system automatically computes the new moving weighted average cost:
  $$\text{New Average} = \frac{(\text{Current Qty} \times \text{Current Avg Price}) + (\text{New Qty} \times \text{Purchase Price})}{\text{Current Qty} + \text{New Qty}}$$
- Creates an immutable audit movement of type `IN`.

### 4. Daily Material Consumption & Depletion Guard (`UcMaterialUsage`)
- Site Managers log daily material usage with specific activity descriptions (e.g., "Foundation column casting").
- The system checks available stock in real time. If usage exceeds stock, the transaction is rejected (Business Rule 1).
- Decrements available stock, records a usage ledger entry, and creates an immutable movement of type `OUT`.
- **Physical Reconciliation**: Admins can record physical inventory counts (`ADJUSTMENT` movement) to reconcile physical surplus or deficit.

### 5. Labor Attendance & Gated Wage Payments (`UcLabor`)
- **Worker Registry**: Register craftsmen (*Aba-byizi*, Masons, Scaffolders, Carpenters) with trade categories and custom daily wage rates.
- **Daily Attendance**: Log attendance for assigned projects (`PRESENT`, `ABSENT`, or `HALF_DAY`).
- **Attendance-Gated Payments (Business Rule 2)**: Click **Pay Daily Wage**. The dialog verifies that the worker is marked `PRESENT`. If `ABSENT`, the payment button is disabled with an explanatory alert. Tracks amount owed vs. amount paid and flags outstanding balances.

### 6. Daily Site Activities & Progress Monitoring (`UcActivities`)
- Log daily site operations (task title, description, date, and progress percentage).
- The dialog features a live slider/progress bar synchronized between 0% and 100%. Values `< 0` or `> 100` are blocked by Technical Rule 5.
- Displays an overall site milestone completion progress bar on the dashboard.

### 7. Management Reporting, CSV Export & PDF Print Preview (`UcReports`)
- Select from 4 report categories:
  1. **Material Consumption & Expenditure Report**
  2. **Labor Wage & Attendance Summary Report**
  3. **Stock Movement Audit Trail (Read-Only)**
  4. **Cross-Project Executive Summary**
- Filter by project and start/end dates.
- View dynamic KPI metric cards (Total Spend, Disbursed Wages, Outstanding Balance, Consumed Quantities).
- **Export to CSV**: Generates RFC 4180 compliant CSV files with UTF-8 BOM for immediate opening in Microsoft Excel.
- **Print / Preview PDF**: Opens a zoomable, paginated print preview with landscape table formatting ready for direct printing or saving as a PDF.

---

## 5. How to Build & Run

### Prerequisites
- Windows 10/11 or Windows Server.
- .NET Framework 4.0 or 4.8 runtime (built-in on modern Windows).
- MSBuild (located at `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe` or Visual Studio developer prompt).

### Building the Solution
Open PowerShell in the project root directory:

```powershell
# Build in Release mode
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe SiteTrack.sln /p:Configuration=Release /t:Rebuild
```

### Running the Desktop Application
```powershell
# Run the compiled executable
.\src\SiteTrack.UI\bin\Release\SiteTrack.exe
```

### Running the Automated Test Suite (76 Tests)
```powershell
# Run the automated verification suite
.\src\SiteTrack.Tests\bin\Release\SiteTrack.Tests.exe
```

### Running via Docker (Cross-Platform)
```bash
# Build and run automated tests in a container
docker-compose up --build sitetrack-tests
```

