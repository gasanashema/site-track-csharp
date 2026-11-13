


SITETRACK 
C O N S T R U C T I O N M A N A G E R ───────────────────────────────────── 
System Requirements Specification
Date 
02 May 2026
Course 
Dot Net
Student ID 
28279
Prepared By 
Shema Philbert Gasana
Institution 
Adventist University of Central Africa (AUCA)















1. Abstract 
SiteTrack Construction Manager is a modern web application designed to help  construction companies efficiently manage multiple active project sites through a centralized, role based platform. The system addresses common operational challenges in the construction industry  including untracked material consumption, poor resource visibility, and informal daily labor  payment practices by providing a structured and reliable digital environment for recording and  monitoring all on-site activities in real time.
The application is built on a backend architecture using Spring Boot. The backend exposes RESTful APIs that handle all business logic, security, and database operations. Data persistence is managed through Spring Boot connected to a relational PostgreSQL database, and the system follows clean architecture principles to ensure a maintainable and scalable codebase.
The platform is built around two core functional domains: material resource management and daily  labor tracking. On the material side, the system supports the full lifecycle of construction resources  from purchase recording and stock allocation per project, to daily consumption logging and a  complete stock movement audit trail ensuring that every unit of material is accounted for from  arrival on site to point of use. On the labor side, the system manages daily workers commonly  known as umubyizi, tracking their attendance per project and recording daily wage payments with  a strict enforcement that no payment record can exist without a corresponding attendance record,  eliminating informal and unverifiable payment practices. 
The system enforces a role-based access model with two primary roles: the Administrator, who  holds full system access and oversees all projects, users, materials, worker types, and financial  summaries across all sites; and the Site Manager, who operates within the scope of their assigned  projects only, recording daily material usage, worker attendance, site activities, and payment  entries. Security is further strengthened through an OTP-based login verification mechanism  delivered via email or SMS simulation, and all notification events are processed through a message  broker (ActiveMQ or RabbitMQ) and logged for traceability. The system also supports reporting with export capabilities to PDF, Excel, and CSV formats, giving management clear and exportable visibility into material consumption trends and labor cost summaries per project.
SiteTrack Construction Manager is designed to bring structure, transparency, and accountability to  construction site operations reducing material wastage through real-time stock monitoring,  formalizing daily labor compensation records, and providing management with reliable, exportable  data to support better decision-making across all active construction projects.
2. Scope 
2.1 Overview 
SiteTrack Construction Manager is scoped to cover the core operational needs of a construction  company managing multiple active project sites. The system is implemented as a backend server application where the Spring Boot service handles all business logic and database operations through RESTful API endpoints. The system focuses on two primary domains: material resource management and daily labor tracking, both anchored to specific construction projects. It is designed for internal use by company staff operating under defined roles and does not extend to client-facing or public-facing functionality.
2.2 In Scope 
User & Access Management 
• Admin and Site Manager role-based access control 
• User account creation, editing, activation, and deactivation by the Admin • Secure login using username and password 
• OTP-based login verification delivered via email or SMS simulation 
• Notification events processed through a message broker (ActiveMQ or RabbitMQ) • OTP verification records stored and validated server-side with expiry enforcement • All notification events logged in the notification_logs table for traceability 
Project Management 
• Creation and management of multiple construction project records 
• Project details including name, location, description, start date, expected end date, and  status 
• Project status tracking: Planning, Ongoing, Completed, Cancelled 
• Assignment of one or more Site Managers to each project 
Material Management 
• Definition and categorization of construction materials such as cement, electrical supplies,  and plumbing materials 
• Recording of material purchases per project including supplier, quantity, and unit price at  time of purchase 
• Maintenance of a real-time material stock balance per project 
• Daily material usage recording by Site Managers 
• Full stock movement audit trail covering IN, OUT, and ADJUSTMENT movements per  project 
• Low stock alerts when a material falls below a defined minimum quantity • Weighted average unit price calculation per material per project
Labor & Attendance Management 
• Registration of daily workers (umubyizi) with worker type classification • Daily attendance recording per worker per project 
• Strict enforcement that payment records are only created for workers marked as present • Daily wage payment recording with a direct one-to-one link to the attendance record • Separation of amount owed and amount paid to support pending and completed payment  states 
• Cumulative labor cost visibility per project 
Site Activity Tracking 
• Daily activity logging per project including title, description, date, and progress  percentage 
• Progress percentage constrained between 0 and 100 
Reporting & Data Export 
• Admin dashboard view with cross-project summaries of material consumption, labor costs,  and project statuses 
• Site Manager view scoped to assigned projects only 
• Per-project material usage reports filterable by date range 
• Per-project labor cost summaries filterable by date range 
• Report export to at least two of the following formats: PDF, Excel, CSV
Technical Architecture 
• Server architecture with a Spring Boot backend • Server application exposes RESTful API endpoints for all CRUD operations
• Spring Boot for all database persistence with proper entity mapping and relationships • Component-based architecture applied across the layered backend controller-service layers
• At least four meaningful Spring Boot entities with proper relationships covering One-to One, One-to-Many, and Many-to-Many 
• Server runs on a port within the range 3000 to 6000 
• At least five business validation rules and five technical validation rules enforced in the system
2.3 Out of Scope 
• Financial accounting, invoicing, billing, or payroll processing 
• Procurement workflows or supplier relationship management
• Integration with third-party construction planning tools such as BIM software or Gantt  chart applications 
• Native mobile apps; the system is a web application accessible via web browsers • GPS or geolocation tracking of workers, equipment, or materials
• Multi-tenant architecture supporting multiple independent companies • Equipment and machinery tracking or maintenance scheduling 
• Document management such as contracts, drawings, or permits 
• Client or stakeholder portal access 
• Automated payroll disbursement or mobile money integration 
• Real-time OTP delivery via a live SMS or email gateway; simulation is sufficient for this  version 
2.4 Target Users 
• Administrator: Has full system access. Responsible for managing users, projects, material  categories, worker types, and system-wide oversight. Can view and export data across all  projects. 
• Site Manager: Has restricted access scoped to assigned projects only. Responsible for  recording daily material usage, worker attendance, activity logs, and worker payment  entries on site. 
2.5 Assumptions & Constraints 
• The system is implemented as a server application with a Spring Boot backend
• The server application exposes all backend endpoints via HTTP/HTTPS REST APIs
• Each construction project is managed by at least one assigned Site Manager • A worker's daily rate is set at registration but can differ from the worker type's default  rate 
• Material prices are captured at the time of purchase to preserve historical accuracy; the  current_price on the materials table serves as a reference only 
• OTP codes are time-limited and marked as used upon successful verification to prevent  reuse 
• The message broker (ActiveMQ or RabbitMQ) is assumed to be running and accessible at  the time of system operation 
• The system is deployed as a single-company internal web tool
• PostgreSQL is used as the relational database management system 
• Spring Boot is configured to connect to the PostgreSQL database named  site_track_construction_manager_db
3. Database Structure 
The system uses a PostgreSQL relational database named site_track_construction_manager_db.  The schema consists of 16 tables as described below.
# Table Columns
1 
users 
id, full_name, username, email, password, role, phone, status,  created_at, updated_at
2 
projects 
id, project_name, location, description, start_date,  
expected_end_date, status, created_by, created_at, updated_at
3 
project_managers 
id, project_id, user_id, assigned_date, status, created_at, updated_at
4 
material_categories 
id, category_name, description, created_at, updated_at
5 
materials 
id, category_id, material_name, unit, current_price, description, status,  created_at, updated_at
6 
material_purchases 
id, project_id, material_id, quantity, unit_price, total_price,  supplier_name, purchase_date, recorded_by, created_at, updated_at
7 
project_material_stock 
id, project_id, material_id, quantity_available, minimum_quantity,  average_unit_price, created_at, updated_at
8 
material_stock_movements 
id, project_id, material_id, movement_type, quantity, unit_price,  total_price, movement_date, description, reference_type,  reference_id, recorded_by, created_at, updated_at
9 
material_usage 
id, project_id, material_id, quantity_used, unit_price, total_cost,  usage_date, activity_description, recorded_by, created_at, updated_at
10 
project_activities 
id, project_id, activity_title, activity_description, activity_date,  progress_percentage, recorded_by, created_at, updated_at
11 
worker_types 
id, type_name, default_daily_rate, description, created_at, updated_at
12 
site_workers 
id, worker_type_id, full_name, phone, daily_rate, status, created_at,  updated_at
13 
worker_attendance 
id, project_id, worker_id, work_date, attendance_status,  work_description, recorded_by, created_at, updated_at
14 
worker_payments 
id, project_id, worker_id, attendance_id, work_date, daily_rate,  amount_owed, amount_paid, payment_status, paid_by, notes,  created_at, updated_at
15 
otp_verifications 
id, user_id, otp_code, expires_at, is_used, created_at
16 
notification_logs 
id, user_id, event_type, message, channel, status, created_at



3.1 Key Constraints & Rules 
• UNIQUE(project_id, material_id) on project_material_stock — one stock record per  material per project 
• UNIQUE(project_id, worker_id, work_date) on worker_attendance — one attendance  record per worker per day per project 
• UNIQUE(attendance_id) on worker_payments — one payment record per attendance  record 
• CHECK(progress_percentage BETWEEN 0 AND 100) on project_activities • worker_payments.attendance_id FK to worker_attendance enforces payment only for  recorded attendance 
• material_stock_movements.reference_type and reference_id link movements back to  their source record (purchase or usage)

4. Functional Requirements 
4.1 User Authentication & Access Management 
1. The system shall allow users to log in using a username and password. 
2. The system shall enforce OTP-based login verification after successful username and  password validation. 
3. The system shall generate a time-limited OTP code and deliver it via email or SMS  simulation upon login attempt. 
4. The system shall invalidate an OTP code after it has been used or after it has expired. 5. The system shall support two roles: Admin and Site Manager. 
6. The system shall restrict access to all system features until OTP verification is successfully  completed. 
7. The system shall log all OTP events and notification dispatches in the notification_logs  table. 
8. All authentication requests shall be processed server-side through exposed REST endpoints.
4.2 User Management 
1. The Admin shall be able to create new user accounts with full name, username, email,  phone, role, and password. 
2. The Admin shall be able to edit existing user account details. 
3. The Admin shall be able to activate or deactivate user accounts. 
4. The Admin shall be able to view a list of all users displayed in a data table.
5. The system shall prevent duplicate usernames and duplicate email addresses across all  user accounts. 
6. The system shall prevent a deactivated user from logging in. 
7. Site Managers shall not have access to user management functionality. 
4.3 Project Management 
1. The Admin shall be able to create new construction project records with project name,  location, description, start date, expected end date, and status. 
2. The Admin shall be able to edit and update existing project records. 
3. The Admin shall be able to change a project status among: Planning, Ongoing, Completed,  and Cancelled. 
4. The Admin shall be able to assign one or more Site Managers to a project. 5. The Admin shall be able to remove a Site Manager from a project. 
6. The system shall display all projects in a data table on the Admin dashboard. 7. Site Managers shall only view projects to which they are assigned.
8. The system shall prevent deletion of a project that has associated material stock, usage  records, or labor records. 
4.4 Material Category Management 
1. The Admin shall be able to create, edit, and view material categories. 
2. Each material category shall have a name and an optional description. 
3. The system shall display all material categories in a data table.
4. The system shall prevent deletion of a category that has materials linked to it. 
4.5 Material Management 
1. The Admin shall be able to create new materials with name, category, unit, current price,  description, and status. 
2. The Admin shall be able to edit and update existing material records. 
3. The Admin shall be able to activate or deactivate materials. 
4. The system shall display all materials in a data table filterable by category.
5. The system shall prevent deletion of a material that has associated purchase or usage  records. 
4.6 Material Purchase Management 
1. The Admin or Site Manager shall be able to record a new material purchase for a specific  project, capturing material, quantity, unit price, total price, supplier name, and purchase  date. 
2. Upon recording a purchase, the system shall automatically update the quantity_available  in the project_material_stock table for the corresponding project and material. 
3. Upon recording a purchase, the system shall automatically create an IN record in the  material_stock_movements table referencing the purchase. 
4. The system shall calculate and store the weighted average unit price in  project_material_stock upon every new purchase. 
5. The system shall display all purchases per project in a data table filterable by date range.
4.7 Material Stock Management 
1. The system shall maintain a real-time stock balance per material per project in the  project_material_stock table. 
2. The system shall enforce a unique constraint of one stock record per material per project. 3. The system shall display current stock levels per project in a data table.
4. The system shall visually highlight materials whose quantity_available has fallen below the  defined minimum_quantity. 
5. The Admin shall be able to record a stock adjustment (ADJUSTMENT movement) to  correct stock after a physical count. 
6. Upon any stock adjustment, the system shall create an ADJUSTMENT record in the  material_stock_movements table.
4.8 Material Usage Management 
1. The Site Manager shall be able to record daily material usage for their assigned project,  capturing material, quantity used, unit price, total cost, usage date, and activity  description. 
2. Upon recording material usage, the system shall automatically reduce the  quantity_available in project_material_stock for the corresponding project and material. 
3. Upon recording material usage, the system shall automatically create an OUT record in the  material_stock_movements table referencing the usage record. 
4. The system shall prevent recording of material usage that exceeds the current  quantity_available in stock. 
5. The system shall display all usage records per project in a data table filterable by date range.
4.9 Stock Movement Audit Trail 
1. The system shall record every stock change — purchase, usage, or adjustment — as an  entry in the material_stock_movements table. 
2. Each movement record shall capture movement type, quantity, unit price, total price,  movement date, description, reference type, reference ID, and the user who recorded it. 
3. The system shall display the full stock movement history per project and per material in a data table.
4. Stock movement records shall be read-only; they cannot be edited or deleted by any user. 
4.10 Site Activity Tracking 
1. The Site Manager shall be able to log daily site activities for their assigned project,  capturing activity title, description, date, and progress percentage. 
2. The system shall enforce that progress percentage is a value between 0 and 100. 3. The Admin shall be able to view activity logs across all projects. 
4. The Site Manager shall only view activity logs for their assigned projects. 5. The system shall display all activity records in a data table filterable by project and date.
4.11 Worker Type Management 
1. The Admin shall be able to create, edit, and view worker types with type name, default  daily rate, and description. 
2. The system shall display all worker types in a data table.
3. The system shall prevent deletion of a worker type that has workers linked to it. 
4.12 Site Worker Management 
1. The Admin or Site Manager shall be able to register a new site worker with full name,  phone, worker type, daily rate, and status. 
2. The system shall pre-fill the daily rate from the worker type's default daily rate but allow  it to be overridden.
3. The Admin or Site Manager shall be able to edit worker details and activate or deactivate  workers. 
4. The system shall display all workers in a data table filterable by worker type and status.
4.13 Worker Attendance Management 
1. The Site Manager shall be able to record daily attendance for workers on their assigned  project, capturing worker, project, work date, attendance status, and work description. 
2. The system shall enforce a unique constraint of one attendance record per worker per  project per work date. 
3. The system shall prevent recording attendance for a worker whose status is INACTIVE. 4. The Admin shall be able to view attendance records across all projects. 
5. The system shall display all attendance records in a data table filterable by project, worker,  and date.
4.14 Worker Payment Management 
1. The Site Manager shall be able to record a daily payment for a worker, but only if an  attendance record marked PRESENT exists for that worker on that date for that project. 
2. The system shall link each payment record directly to its corresponding attendance record  through the attendance_id foreign key. 
3. The system shall pre-fill the daily rate and amount owed from the worker's registered  daily rate. 
4. The Site Manager shall be able to update the payment status from PENDING to PAID and  record the amount paid. 
5. The system shall prevent creating more than one payment record per attendance record. 6. The Admin shall be able to view all payment records across all projects. 
7. The system shall display all payment records in a data table filterable by project, worker,  payment status, and date range.
4.15 Reporting & Data Export 
1. The system shall generate a per-project material usage report showing material name,  quantity used, unit price, total cost, and usage date, filterable by date range. 
2. The system shall generate a per-project labor cost summary report showing worker name,  work date, daily rate, amount owed, amount paid, and payment status, filterable by date  range. 
3. The system shall generate a stock movement report per project showing all IN, OUT, and  ADJUSTMENT movements with full details. 
4. The Admin shall be able to view a cross-project summary report showing total material  expenditure and total labor cost per project. 
5. All report data shall be made available via REST endpoints for export.
6. The system shall support export of any report to at least two of the following formats:  PDF, Excel, CSV.
4.16 Notification & Messaging 
1. The system shall use a message broker (ActiveMQ or RabbitMQ) to process and dispatch  all notification events. 
2. The system shall send an OTP notification to the user via email or SMS simulation upon  every login attempt. 
3. The system shall log every notification event in the notification_logs table, capturing user,  event type, message, channel, and delivery status. 
4. The system shall notify the Admin when a material stock level falls below the defined  minimum quantity for a project. 
5. All notification dispatch logic shall be handled server-side and shall not be accessible  directly from the client. 
4.17 Validation Rules 
Business Validation Rules 
1. A material usage entry shall be rejected if the quantity to be used exceeds the current  available stock for that material on that project. 
2. A worker payment record shall only be created if the worker has an attendance record  with status PRESENT for the same project and work date. 
3. An OTP code shall be rejected if it has already been used or if it has passed its expiry  timestamp. 
4. A Site Manager shall not be able to view, record, or modify data belonging to a project  they are not assigned to. 
5. A project shall not be cancelled or deleted if it has active ongoing material stock  movements or pending worker payments. 
Technical Validation Rules 
1. All required request fields shall be validated as non-empty before processing; error messages shall be returned for any missing required field.
2. Numeric fields such as quantity, price, daily rate, and progress percentage shall be  validated to accept only positive numeric values of the correct data type. 
3. Date fields shall be validated to ensure they are in the correct format and that end dates  are not set before start dates. 
4. Username and email fields shall be validated for uniqueness against existing records  before a new user is saved. 
5. Progress percentage shall be validated to be an integer value strictly between 0 and 100  inclusive before the record is persisted to the database.
6. Class Diagram (Conceptual Overview) 
The following table describes the core .NET entity classes, their key attributes, and their  relationships within the SiteTrack system. These entities form the backbone of the data model and  map directly to the PostgreSQL database tables. 

6.1 Relationship Summary 
• One-to-Many: User → Projects (creates), Project → MaterialPurchases, Project → WorkerAttendances, MaterialCategory → Materials, WorkerType → SiteWorkers, User → OtpVerifications, User → NotificationLogs 
• Many-to-Many: Projects ↔ Users (Site Managers) through ProjectManager join table 
• One-to-One: WorkerAttendance ↔ WorkerPayment (enforced via UNIQUE attendance_id  FK on worker_payments) 
• Audit reference: MaterialStockMovement links back to its source via referenceType  ('purchase' or 'usage') and referenceId pointing to the originating record
